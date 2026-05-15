using WebApiRRHH.DTOs;
using WebApiRRHH.Models;
using WebApiRRHH.Repositories.Interfaces;

namespace WebApiRRHH.Services
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<EmpleadoResponseDto>> GetAllEmpleadosAsync();
        Task<EmpleadoResponseDto?> GetEmpleadoByIdAsync(int id);
        Task<EmpleadoResponseDto> CreateEmpleadoAsync(CreateEmpleadoDto createEmpleadoDto);
        Task<EmpleadoResponseDto?> UpdateEmpleadoAsync(int id, UpdateEmpleadoDto updateEmpleadoDto);
        Task<bool> DeleteEmpleadoAsync(int id);
    }
    public class EmpleadoService: IEmpleadoService
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly ILogger<EmpleadoService> _logger;

        public EmpleadoService(IEmpleadoRepository empleadoRepository, ILogger<EmpleadoService> logger)
        {
            _empleadoRepository = empleadoRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmpleadoResponseDto>> GetAllEmpleadosAsync()
        {
            var empleados = await _empleadoRepository.GetAllAsync();
            return empleados.Select(MapToResponseDto);
        }

        public async Task<EmpleadoResponseDto?> GetEmpleadoByIdAsync(int id)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(id);
            return empleado == null ? null : MapToResponseDto(empleado);
        }

        public async Task<EmpleadoResponseDto> CreateEmpleadoAsync(CreateEmpleadoDto createEmpleadoDto)
        {
            // Validar que el email no exista
            if (await _empleadoRepository.EmailExistsAsync(createEmpleadoDto.Email))
            {
                throw new InvalidOperationException($"El email {createEmpleadoDto.Email} ya está registrado");
            }

            var empleado = new Empleado
            {
                Name = createEmpleadoDto.Name,
                Email = createEmpleadoDto.Email,
                PhoneNumber = createEmpleadoDto.PhoneNumber,
                IsActive = true
            };

            var createdEmpleado = await _empleadoRepository.CreateAsync(empleado);
            _logger.LogInformation("Nuevo usuario creado: {Email}", createdEmpleado.Email);

            return MapToResponseDto(createdEmpleado);
        }

        public async Task<EmpleadoResponseDto?> UpdateEmpleadoAsync(int id, UpdateEmpleadoDto updateEmpleadoDto)
        {
            var existingEmpleado = await _empleadoRepository.GetByIdAsync(id);
            if (existingEmpleado == null)
                return null;

            // Validar email si se está actualizando
            if (!string.IsNullOrWhiteSpace(updateEmpleadoDto.Email) &&
                updateEmpleadoDto.Email != existingEmpleado.Email)
            {
                if (await _empleadoRepository.EmailExistsAsync(updateEmpleadoDto.Email, id))
                {
                    throw new InvalidOperationException($"El email {updateEmpleadoDto.Email} ya está registrado");
                }
                existingEmpleado.Email = updateEmpleadoDto.Email;
            }

            // Actualizar solo los campos que no son null
            if (!string.IsNullOrWhiteSpace(updateEmpleadoDto.Name))
                existingEmpleado.Name = updateEmpleadoDto.Name;

            if (updateEmpleadoDto.IsActive.HasValue)
                existingEmpleado.IsActive = updateEmpleadoDto.IsActive.Value;

            var updatedEmpleado = await _empleadoRepository.UpdateAsync(existingEmpleado);
            return MapToResponseDto(updatedEmpleado);
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            return await _empleadoRepository.DeleteAsync(id);
        }

        // Mapeo de entidad a DTO
        private EmpleadoResponseDto MapToResponseDto(Empleado empleado)
        {
            return new EmpleadoResponseDto
            {
                Id = empleado.Id,
                Name = empleado.Name,
                //LastName = empleado.LastName,
                //FullName = empleado.FullName,
                Email = empleado.Email,
                PhoneNumber = empleado.PhoneNumber,
                IsActive = empleado.IsActive,
                CreatedAt = empleado.CreatedAt,
                UpdatedAt = empleado.UpdatedAt
            };
        }

    }
}
