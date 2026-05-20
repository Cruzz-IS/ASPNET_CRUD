using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApiRRHH.Configuration;
using WebApiRRHH.Context;
using WebApiRRHH.DTOs.Auth;
using WebApiRRHH.Models;
using WebApiRRHH.Services.Security;

namespace WebApiRRHH.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress, string empleadoAgent);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string empleadoAgent);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string ipAddress, string empleadoAgent);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ChangePasswordAsync(int empleadoId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ConfirmEmailAsync(int empleadoId, string token);
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
            => (_context, _passwordHasher, _jwtService, _jwtSettings, _securitySettings, _logger, _auditService)
            = (context, passwordHasher, jwtService, jwtSettings, securitySettings, logger, auditService);

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress, string empleadoAgent)
        {
            try
            {
                // Verificar si el email ya existe, si es el caso no dejara registrar al cliente
                if (await _context.Empleados!.AnyAsync(u => string.Equals(u.Email, registerDto.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "El email ya está registrado"
                    };
                }

                // Crear usuario
                var empleado = new Empleado
                {
                    Name = registerDto.Name,
                    //LastName = registerDto.LastName,
                    Email = registerDto.Email.ToLower(),
                    PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = "Cliente", // Rol por defecto
                    IsActive = true,
                    EmailConfirmed = !_securitySettings.RequireEmailConfirmation,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedDate = DateTime.UtcNow
                };

                _context.Empleados!.Add(empleado);
                await _context.SaveChangesAsync();

                // Registrar en auditoría
                await _auditService.LogAsync("Register", "Empleado", empleado.Id, null,
                    $"Usuario registrado: {empleado.Email}", ipAddress, empleadoAgent, empleado.Id);

                _logger.LogInformation("Usuario registrado exitosamente: {Email}", empleado.Email);

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
                return await GenerateAuthResponseAsync(empleado, ipAddress, empleadoAgent);
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

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress, string empleadoAgent)
        {
            try
            {
                var empleado = await _context.Empleados!
                    .FirstOrDefaultAsync(u => string.Equals(u.Email, loginDto.Email, StringComparison.OrdinalIgnoreCase));

                if (empleado == null)
                {
                    await _auditService.LogAsync("LoginFailed", "Empleado", null, null,
                        $"Intento de login con email no existente: {loginDto.Email}",
                        ipAddress, empleadoAgent, null, "Warning");

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar si la cuenta está bloqueada
                if (empleado.IsLockedOut)
                {
                    await _auditService.LogAsync("LoginBlocked", "Empleado", empleado.Id, null,
                        $"Intento de login en cuenta bloqueada: {empleado.Email}",
                        ipAddress, empleadoAgent, empleado.Id, "Warning");

                    var remainingTime = (empleado.LockoutEnd!.Value - DateTime.UtcNow).Minutes;
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = $"Cuenta bloqueada. Intente nuevamente en {remainingTime} minutos."
                    };
                }

                // Verificar contraseña
                if (!_passwordHasher.VerifyPassword(loginDto.Password, empleado.PasswordHash))
                {
                    // Incrementar intentos fallidos para guardar la cantidad de veces que se a intentado loguear un usuario
                    empleado.FailedLoginAttempts++;

                    if (empleado.FailedLoginAttempts >= _securitySettings.MaxLoginAttempts)
                    {
                        empleado.LockoutEnd = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutMinutes);

                        await _auditService.LogAsync("AccountLocked", "Empleado", empleado.Id, null,
                            $"Cuenta bloqueada por intentos fallidos: {empleado.Email}",
                            ipAddress, empleadoAgent, empleado.Id, "Warning");

                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", empleado.Email);
                    }

                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync("LoginFailed", "Empleado", empleado.Id, null,
                        $"Contraseña incorrecta para: {empleado.Email}",
                        ipAddress, empleadoAgent, empleado.Id, "Warning");

                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar si el usuario esta activo
                if (!empleado.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Cuenta desactivada. Contacte al administrador."
                    };
                }

                // Verificar confirmación de email si está habilitada
                if (_securitySettings.RequireEmailConfirmation && !empleado.EmailConfirmed)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Debe confirmar su email antes de iniciar sesión."
                    };
                }

                // Login exitoso - resetear intentos fallidos
                empleado.FailedLoginAttempts = 0;
                empleado.LockoutEnd = null;
                empleado.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Login", "Empleado", empleado.Id, null,
                    $"Login exitoso: {empleado.Email}", ipAddress, empleadoAgent, empleado.Id);

                _logger.LogInformation("Login exitoso: {Email}", empleado.Email);

                return await GenerateAuthResponseAsync(empleado, ipAddress, empleadoAgent);
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

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string ipAddress, string empleadoAgent)
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

                var empleadoIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(empleadoIdClaim, out int empleadoId))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token inválido"
                    };
                }

                // Buscar el refresh token
                var storedRefreshToken = await _context.RefreshTokens!
                    .Include(rt => rt.Empleado)
                    .FirstOrDefaultAsync(rt =>
                        rt.Token == refreshTokenDto.RefreshToken &&
                        rt.EmpleadoId == empleadoId);

                if (storedRefreshToken == null || !storedRefreshToken.IsActive)
                {
                    await _auditService.LogAsync("RefreshTokenFailed", "RefreshToken", null, null,
                        $"Intento de usar refresh token inválido", ipAddress, empleadoAgent, empleadoId, "Warning");

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
                var response = await GenerateAuthResponseAsync(storedRefreshToken.Empleado, ipAddress, empleadoAgent);

                await _auditService.LogAsync("RefreshToken", "Empleado", empleadoId, null,
                    $"Tokens renovados exitosamente", ipAddress, empleadoAgent, empleadoId);

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
                    $"Refresh token revocado", ipAddress, null, token.EmpleadoId);

                _logger.LogInformation("Refresh token revocado para usuario: {EmpleadoId}", token.EmpleadoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar token");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int empleadoId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var empleado = await _context.Empleados!.FindAsync(empleadoId);
                if (empleado == null)
                    return false;

                // Verificar contraseña actual
                if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, empleado.PasswordHash))
                {
                    await _auditService.LogAsync("ChangePasswordFailed", "Empleado", empleadoId, null,
                        "Contraseña actual incorrecta", null, null, empleadoId, "Warning");
                    return false;
                }

                // Actualizar contraseña
                empleado.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
                empleado.PasswordChangedDate = DateTime.UtcNow;
                empleado.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ChangePassword", "Empleado", empleadoId, null,
                    "Contraseña cambiada exitosamente", null, null, empleadoId);

                _logger.LogInformation("Contraseña cambiada para usuario: {EmpleadoId}", empleadoId);
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
                var empleado = await _context.Empleados!
                    .FirstOrDefaultAsync(u => string.Equals(u.Email, forgotPasswordDto.Email, StringComparison.OrdinalIgnoreCase));

                if (empleado == null)
                {
                    // No revelar si el email existe o no esto por tema de seguridad
                    return true;
                }

                // Generar token de reset
                var resetToken = _passwordHasher.GenerateSecureToken();
                empleado.ResetPasswordToken = resetToken;
                empleado.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ForgotPassword", "Empleado", empleado.Id, null,
                    "Token de reset de contraseña generado", null, null, empleado.Id);

                //Enviar email con el token
                _logger.LogInformation("Token de reset generado para: {Email}", empleado.Email);

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
                var empleado = await _context.Empleados!
                    .FirstOrDefaultAsync(u =>
                        string.Equals(u.Email, resetPasswordDto.Email, StringComparison.OrdinalIgnoreCase) &&
                        u.ResetPasswordToken == resetPasswordDto.Token &&
                        u.ResetPasswordTokenExpiry > DateTime.UtcNow);

                if (empleado == null)
                {
                    await _auditService.LogAsync("ResetPasswordFailed", "Empleado", null, null,
                        $"Token de reset inválido o expirado para: {resetPasswordDto.Email}",
                        null, null, null, "Warning");
                    return false;
                }

                // Actualizar contraseña
                empleado.PasswordHash = _passwordHasher.HashPassword(resetPasswordDto.NewPassword);
                empleado.PasswordChangedDate = DateTime.UtcNow;
                empleado.ResetPasswordToken = null;
                empleado.ResetPasswordTokenExpiry = null;
                empleado.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ResetPassword", "Empleado", empleado.Id, null,
                    "Contraseña reseteada exitosamente", null, null, empleado.Id);

                _logger.LogInformation("Contraseña reseteada para: {Email}", empleado.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña");
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(int empleadoId, string token)
        {
            try
            {
                var empleado = await _context.Empleados!.FindAsync(empleadoId);
                if (empleado == null || empleado.EmailConfirmed)
                    return false;

                // Validar token de confirmación

                empleado.EmailConfirmed = true;
                empleado.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync("ConfirmEmail", "Empleado", empleadoId, null,
                    "Email confirmado", null, null, empleadoId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar email");
                return false;
            }
        }

        // Método privado para generar la respuesta de autenticación
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(Empleado empleado, string ipAddress, string empleadoAgent)
        {
            var accessToken = _jwtService.GenerateAccessToken(empleado);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Guardar refresh token en la BD
            var refreshTokenEntity = new RefreshToken
            {
                EmpleadoId = empleado.Id,
                Token = refreshToken,
                JwtId = GetJwtId(accessToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                IpAddress = ipAddress,
                EmpleadoAgent = empleadoAgent
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
                Empleado = new EmpleadoInfoDto
                {
                    Id = empleado.Id,
                    Email = empleado.Email,
                    Name = empleado.Name,
                    Role = empleado.Role,
                    EmailConfirmed = empleado.EmailConfirmed
                }
            };
        }

        private static string GetJwtId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        }
    }
}