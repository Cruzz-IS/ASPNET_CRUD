using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiRRHH.DTOs.Auth;
using WebApiRRHH.Services;
using System.Security.Claims;

namespace WebApiRRHH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registrar un nuevo empleado
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var empleadoAgent = GetEmpleadoAgent();

                var result = await _authService.RegisterAsync(registerDto, ipAddress, empleadoAgent);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de empleado");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Iniciar sesión
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var empleadoAgent = GetEmpleadoAgent();

                var result = await _authService.LoginAsync(loginDto, ipAddress, empleadoAgent);

                if (!result.Success)
                    return Unauthorized(result);

                // Configurar cookie con el refresh token, mas seguro para evitar que roben de datos del empleado.
                SetRefreshTokenCookie(result.RefreshToken!);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Renovar tokens usando el refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // Si el refresh token no viene en el body, intentar obtenerlo de la cookie
                if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
                {
                    refreshTokenDto.RefreshToken = Request.Cookies["refreshToken"] ?? "";
                }

                if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Refresh token requerido"
                    });
                }

                var ipAddress = GetIpAddress();
                var empleadoAgent = GetEmpleadoAgent();

                var result = await _authService.RefreshTokenAsync(refreshTokenDto, ipAddress, empleadoAgent);

                if (!result.Success)
                    return Unauthorized(result);

                SetRefreshTokenCookie(result.RefreshToken!);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar token");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Cerrar sesión y revocar refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RevokeTokenDto? revokeTokenDto)
        {
            try
            {
                var refreshToken = revokeTokenDto?.RefreshToken ?? Request.Cookies["refreshToken"];

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return BadRequest(new { message = "Refresh token requerido" });
                }

                var ipAddress = GetIpAddress();
                var result = await _authService.RevokeTokenAsync(refreshToken, ipAddress);

                if (!result)
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                Response.Cookies.Delete("refreshToken");

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en logout");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambiar contraseña solo cuando el empleado esta autenticado
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var empleadoId = GetCurrentEmpleadoId();
                if (!empleadoId.HasValue)
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var result = await _authService.ChangePasswordAsync(empleadoId.Value, changePasswordDto);

                if (!result)
                {
                    return BadRequest(new { message = "No se pudo cambiar la contraseña. Verifique la contraseña actual." });
                }

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Solicitar reset de contraseña (forgot password)
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _authService.ForgotPasswordAsync(forgotPasswordDto);

                // Siempre retornamos un success para no revelar si el email existe a cualquier empleado
                return Ok(new
                {
                    message = "Si el email existe, recibirá un enlace para resetear su contraseña"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en forgot password");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Resetear contraseña con token
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.ResetPasswordAsync(resetPasswordDto);

                if (!result)
                {
                    return BadRequest(new { message = "Token inválido o expirado" });
                }

                return Ok(new { message = "Contraseña reseteada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener información del empleado actual
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(EmpleadoInfoDto), StatusCodes.Status200OK)]
        public IActionResult GetCurrentEmpleado()
        {
            try
            {
                var empleadoId = GetCurrentEmpleadoId();
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var department = User.FindFirst("department")?.Value;
                var position = User.FindFirst("position")?.Value;

                if (!empleadoId.HasValue)
                    return Unauthorized();

                var EmpleadoInfo = new EmpleadoInfoDto
                {
                    Id = empleadoId.Value,
                    Email = email ?? "",
                    Name = name ?? "",
                    Role = role ?? ""
                };

                return Ok(EmpleadoInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Métodos auxiliares privados
        private int? GetCurrentEmpleadoId()
        {
            var empleadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(empleadoIdClaim, out int empleadoId) ? empleadoId : null;
        }

        private string GetIpAddress()
        {
            // Intentar obtener IP real detrás de proxy
            var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }

            return ipAddress;
        }

        private string GetEmpleadoAgent()
        {
            return HttpContext.Request.Headers["Empleado-Agent"].FirstOrDefault() ?? "Unknown";
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = true,   // Solo HTTPS esto cuando se lanza a produccion
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}