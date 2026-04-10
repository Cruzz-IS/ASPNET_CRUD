using System.ComponentModel.DataAnnotations;

namespace WebApiRRHH.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }
    }

    // DTO para actualizar un usuario
    public class UpdateUserDto
    {
        [StringLength(80, MinimumLength = 2)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
    }

    // DTO de respuesta (lo que devolvemos al cliente Frontend)
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
