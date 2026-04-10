using WebApiRRHH.DTOs;
using WebApiRRHH.Models;
using WebApiRRHH.Repositories.Interfaces;

namespace WebApiRRHH.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
    }
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToResponseDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Validar que el email no exista
            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
            {
                throw new InvalidOperationException($"El email {createUserDto.Email} ya está registrado");
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.PhoneNumber,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            _logger.LogInformation("Nuevo usuario creado: {Email}", createdUser.Email);

            return MapToResponseDto(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                return null;

            // Validar email si se está actualizando
            if (!string.IsNullOrWhiteSpace(updateUserDto.Email) &&
                updateUserDto.Email != existingUser.Email)
            {
                if (await _userRepository.EmailExistsAsync(updateUserDto.Email, id))
                {
                    throw new InvalidOperationException($"El email {updateUserDto.Email} ya está registrado");
                }
                existingUser.Email = updateUserDto.Email;
            }

            // Actualizar solo los campos que no son null
            if (!string.IsNullOrWhiteSpace(updateUserDto.Name))
                existingUser.Name = updateUserDto.Name;

            if (updateUserDto.IsActive.HasValue)
                existingUser.IsActive = updateUserDto.IsActive.Value;

            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            return MapToResponseDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        // Mapeo de entidad a DTO
        private UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                //LastName = user.LastName,
                //FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

    }
}
