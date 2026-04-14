using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApiRRHH.Configuration;
using WebApiRRHH.Context;
using WebApiRRHH.DTOs.Auth;
using WebApiRRHH.Models;
using WebApiRRHH.Services.Security;

namespace WebApiRRHH.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress, string userAgent);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string userAgent);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string ipAddress, string userAgent);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ConfirmEmailAsync(int userId, string token);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDBContext _context;
        private readonly IPasswordHash _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditService _auditService;

        public AuthService(
            AppDBContext context,
            IPasswordHash passwordHasher,
            IJwtService jwtService,
            JwtSettings jwtSettings,
            SecuritySettings securitySettings,
            ILogger<AuthService> logger,
            IAuditService auditService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings;
            _securitySettings = securitySettings;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress, string userAgent)
        {
            try
            {
                // Verificar si el email ya existe, si es el caso no dejara registrar al cliente
                if (await _context.Users!.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower()))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "El email ya está registrado"
                    };
                }

                // Crear usuario
                var user = new User
                {
                    Name = registerDto.FirstName,
                    //LastName = registerDto.LastName,
                    Email = registerDto.Email.ToLower(),
                    PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = "Employee", // Rol por defecto
                    IsActive = true,
                    EmailConfirmed = !_securitySettings.RequireEmailConfirmation,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedDate = DateTime.UtcNow
                };

                _context.Users!.Add(user);
                await _context.SaveChangesAsync();

                // Registrar en auditoría
                await _auditService.LogAsync("Register", "User", user.Id, null,
                    $"Usuario registrado: {user.Email}", ipAddress, userAgent, user.Id);

                _logger.LogInformation("Usuario registrado exitosamente: {Email}", user.Email);

                // Si requiere confirmación de email, enviar email (implementar después)
                if (_securitySettings.RequireEmailConfirmation)
                {
                    // TODO: Enviar email de confirmación
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Usuario registrado. Por favor, confirme su email."
                    };
                }

                // Generar tokens
                return await GenerateAuthResponseAsync(user, ipAddress, userAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al registrar usuario"
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string userAgent)
        {
            try
            {
                var user = await _context.Users!
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

                if (user == null)
                {
                    await _auditService.LogAsync("LoginFailed", "User", null, null,
                        $"Intento de login con email no existente: {loginDto.Email}",
                        ipAddress, userAgent, null, "Warning");

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar si la cuenta está bloqueada
                if (user.IsLockedOut)
                {
                    await _auditService.LogAsync("LoginBlocked", "User", user.Id, null,
                        $"Intento de login en cuenta bloqueada: {user.Email}",
                        ipAddress, userAgent, user.Id, "Warning");

                    var remainingTime = (user.LockoutEnd!.Value - DateTime.UtcNow).Minutes;
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = $"Cuenta bloqueada. Intente nuevamente en {remainingTime} minutos."
                    };
                }

                // Verificar contraseña
                if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    // Incrementar intentos fallidos para guardar la cantidad de veces que se a intentado loguear un usuario
                    user.FailedLoginAttempts++;

                    if (user.FailedLoginAttempts >= _securitySettings.MaxLoginAttempts)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutMinutes);

                        await _auditService.LogAsync("AccountLocked", "User", user.Id, null,
                            $"Cuenta bloqueada por intentos fallidos: {user.Email}",
                            ipAddress, userAgent, user.Id, "Warning");

                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", user.Email);
                    }

                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync("LoginFailed", "User", user.Id, null,
                        $"Contraseña incorrecta para: {user.Email}",
                        ipAddress, userAgent, user.Id, "Warning");

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar si el usuario esta activo
                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Cuenta desactivada. Contacte al administrador."
                    };
                }

                // Verificar confirmación de email si está habilitada
                if (_securitySettings.RequireEmailConfirmation && !user.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Debe confirmar su email antes de iniciar sesión."
                    };
                }

                // Login exitoso - resetear intentos fallidos
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Login", "User", user.Id, null,
                    $"Login exitoso: {user.Email}", ipAddress, userAgent, user.Id);

                _logger.LogInformation("Login exitoso: {Email}", user.Email);

                return await GenerateAuthResponseAsync(user, ipAddress, userAgent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al iniciar sesión"
                };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string ipAddress, string userAgent)
        {
            try
            {
                // Validar el access token expirado
                var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
                if (principal == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token inválido"
                    };
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token inválido"
                    };
                }

                // Buscar el refresh token
                var storedRefreshToken = await _context.RefreshTokens!
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt =>
                        rt.Token == refreshTokenDto.RefreshToken &&
                        rt.UserId == userId);

                if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                {
                    await _auditService.LogAsync("RefreshTokenFailed", "RefreshToken", null, null,
                        $"Intento de usar refresh token inválido", ipAddress, userAgent, userId, "Warning");

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Refresh token inválido o expirado"
                    };
                }

                // Marcar el refresh token antiguo como usado
                storedRefreshToken.IsUsed = true;
                await _context.SaveChangesAsync();

                // Generar nuevos tokens, actualizara el refresh token
                var response = await GenerateAuthResponseAsync(storedRefreshToken.User, ipAddress, userAgent);

                await _auditService.LogAsync("RefreshToken", "User", userId, null,
                    $"Tokens renovados exitosamente", ipAddress, userAgent, userId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar tokens");
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Error al renovar tokens"
                };
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var token = await _context.RefreshTokens!
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token == null || !token.IsActive)
                    return false;

                token.IsRevoked = true;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("RevokeToken", "RefreshToken", token.Id, null,
                    $"Refresh token revocado", ipAddress, null, token.UserId);

                _logger.LogInformation("Refresh token revocado para usuario: {UserId}", token.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar token");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users!.FindAsync(userId);
                if (user == null)
                    return false;

                // Verificar contraseña actual
                if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    await _auditService.LogAsync("ChangePasswordFailed", "User", userId, null,
                        "Contraseña actual incorrecta", null, null, userId, "Warning");
                    return false;
                }

                // Actualizar contraseña
                user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
                user.PasswordChangedDate = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ChangePassword", "User", userId, null,
                    "Contraseña cambiada exitosamente", null, null, userId);

                _logger.LogInformation("Contraseña cambiada para usuario: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _context.Users!
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == forgotPasswordDto.Email.ToLower());

                if (user == null)
                {
                    // No revelar si el email existe o no esto por tema de seguridad
                    return true;
                }

                // Generar token de reset
                var resetToken = _passwordHasher.GenerateSecureToken();
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ForgotPassword", "User", user.Id, null,
                    "Token de reset de contraseña generado", null, null, user.Id);

                //Enviar email con el token
                _logger.LogInformation("Token de reset generado para: {Email}", user.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en forgot password");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _context.Users!
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == resetPasswordDto.Email.ToLower() &&
                        u.ResetPasswordToken == resetPasswordDto.Token &&
                        u.ResetPasswordTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    await _auditService.LogAsync("ResetPasswordFailed", "User", null, null,
                        $"Token de reset inválido o expirado para: {resetPasswordDto.Email}",
                        null, null, null, "Warning");
                    return false;
                }

                // Actualizar contraseña
                user.PasswordHash = _passwordHasher.HashPassword(resetPasswordDto.NewPassword);
                user.PasswordChangedDate = DateTime.UtcNow;
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ResetPassword", "User", user.Id, null,
                    "Contraseña reseteada exitosamente", null, null, user.Id);

                _logger.LogInformation("Contraseña reseteada para: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña");
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(int userId, string token)
        {
            try
            {
                var user = await _context.Users!.FindAsync(userId);
                if (user == null || user.EmailConfirmed)
                    return false;

                // Validar token de confirmación

                user.EmailConfirmed = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ConfirmEmail", "User", userId, null,
                    "Email confirmado", null, null, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar email");
                return false;
            }
        }

        // Método privado para generar la respuesta de autenticación
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, string ipAddress, string userAgent)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Guardar refresh token en la BD
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                JwtId = GetJwtId(accessToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.RefreshTokens!.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Autenticación exitosa",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role,
                    EmailConfirmed = user.EmailConfirmed
                }
            };
        }

        private string GetJwtId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        }
    }
}