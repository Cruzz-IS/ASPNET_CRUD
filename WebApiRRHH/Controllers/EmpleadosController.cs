using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiRRHH.DTOs;
using WebApiRRHH.Services;

namespace WebApiRRHH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly ILogger<EmpleadosController> _logger;

        public EmpleadosController(IEmpleadoService empleadoService, ILogger<EmpleadosController> logger)
        {
            _empleadoService = empleadoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los empleados activos
        /// </summary>
        /// <returns>Lista de empleados</returns>
        /// <response code="200">Retorna la lista de empleados</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmpleadoResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmpleadoResponseDto>>> GetEmpleados()
        {
            try
            {
                var empleados = await _empleadoService.GetAllEmpleadosAsync();
                return Ok(empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de empleados");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un empleado por su ID
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Empleado encontrado</returns>
        /// <response code="200">Empleado encontrado</response>
        /// <response code="404">Empleado no encontrado</response>
        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmpleadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoResponseDto>> GetEmpleado(int id)
        {
            try
            {
                var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);

                if (empleado == null)
                {
                    _logger.LogWarning("Empleado con ID {EmpleadoId} no encontrado", id);
                    return NotFound(new { message = $"Empleado con ID {id} no encontrado" });
                }

                return Ok(empleado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado con ID {EmpleadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo empleado
        /// </summary>
        /// <param name="createEmpleadoDto">Datos del empleado a crear</param>
        /// <returns>Empleado creado</returns>
        /// <response code="201">Empleado creado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(EmpleadoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmpleadoResponseDto>> CreateEmpleado([FromBody] CreateEmpleadoDto createEmpleadoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var empleado = await _empleadoService.CreateEmpleadoAsync(createEmpleadoDto);

                return CreatedAtAction(
                    nameof(GetEmpleado),
                    new { id = empleado.Id },
                    empleado
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear empleado");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empleado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un empleado existente
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <param name="updateEmpleadoDto">Datos a actualizar</param>
        /// <returns>Empleado actualizado</returns>
        /// <response code="200">Empleado actualizado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="404">Empleado no encontrado</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmpleadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmpleadoResponseDto>> UpdateEmpleado(int id, [FromBody] UpdateEmpleadoDto updateEmpleadoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var empleado = await _empleadoService.UpdateEmpleadoAsync(id, updateEmpleadoDto);

                if (empleado == null)
                {
                    _logger.LogWarning("Intento de actualizar empleado inexistente: ID {EmpleadoId}", id);
                    return NotFound(new { message = $"Empleado con ID {id} no encontrado" });
                }

                return Ok(empleado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar empleado");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empleado con ID {EmpleadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }

        }

        /// <summary>
        /// Elimina (desactiva) un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="204">Empleado eliminado exitosamente</response>
        /// <response code="404">Empleado no encontrado</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            try
            {
                var result = await _empleadoService.DeleteEmpleadoAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Intento de eliminar empleado inexistente: ID {EmpleadoId}", id);
                    return NotFound(new { message = $"Empleado con ID {id} no encontrado" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empleado con ID {EmpleadoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

}