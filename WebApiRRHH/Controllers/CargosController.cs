using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiRRHH.DTOs;
using WebApiRRHH.Services;

namespace WebApiRRHH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CargoController : ControllerBase
    {
        private readonly ICargoService _cargoService;
        private readonly ILogger<CargosController> _logger;

        public CargoController(ICargoService cargoService, ILogger<CargosController> logger)
        {
            _cargoService = cargoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los cargos activos
        /// </summary>
        /// <returns>Lista de cargos</returns>
        /// <response code="200">Retorna la lista de cargos</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CargoResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CargoResponseDto>>> GetCargos()
        {
            try
            {
                var cargos = await _cargoService.GetAllCargosAsync();
                return Ok(cargos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de cargos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un cargo por su ID
        /// </summary>
        /// <param name="id">ID del cargo</param>
        /// <returns>Cargo encontrado</returns>
        /// <response code="200">Cargo encontrado</response>
        /// <response code="404">Cargo no encontrado</response>
        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CargoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CargoResponseDto>> GetCargo(int id)
        {
            try
            {
                var cargo = await _cargoService.GetCargoByIdAsync(id);

                if (cargo == null)
                {
                    _logger.LogWarning("Cargo con ID {CargoId} no encontrado", id);
                    return NotFound(new { message = $"Cargo con ID {id} no encontrado" });
                }

                return Ok(cargo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cargo con ID {CargoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo cargo
        /// </summary>
        /// <param name="createCargoDto">Datos del cargo a crear</param>
        /// <returns>Cargo creado</returns>
        /// <response code="201">Cargo creado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(CargoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CargoResponseDto>> CreateCargo([FromBody] CreateCargoDto createCargoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cargo = await _cargoService.CreateCargoAsync(createCargoDto);

                return CreatedAtAction(
                    nameof(GetCargo),
                    new { id = cargo.Id },
                    cargo
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear cargo");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cargo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un cargo existente
        /// </summary>
        /// <param name="id">ID del cargo</param>
        /// <param name="updateCargoDto">Datos a actualizar</param>
        /// <returns>Cargo actualizado</returns>
        /// <response code="200">Cargo actualizado exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="404">Cargo no encontrado</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CargoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CargoResponseDto>> UpdateCargo(int id, [FromBody] UpdateCargoDto updateCargoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cargo = await _cargoService.UpdateCargoAsync(id, updateCargoDto);

                if (cargo == null)
                {
                    _logger.LogWarning("Intento de actualizar cargo inexistente: ID {CargoId}", id);
                    return NotFound(new { message = $"Cargo con ID {id} no encontrado" });
                }

                return Ok(cargo);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar cargo");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cargo con ID {CargoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }

        }

        /// <summary>
        /// Elimina (desactiva) un cargo
        /// </summary>
        /// <param name="id">ID del cargo</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="204">Cargo eliminado exitosamente</response>
        /// <response code="404">Cargo no encontrado</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCargo(int id)
        {
            try
            {
                var result = await _cargoService.DeleteCargoAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Intento de eliminar cargo inexistente: ID {CargoId}", id);
                    return NotFound(new { message = $"Cargo con ID {id} no encontrado" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cargo con ID {CargoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

}