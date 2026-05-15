using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    [Table("Empleado")]
    public class Empleado : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;


        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(13, MinimumLength = 1, ErrorMessage = "El DNI debe tener entre 1 y 13 caracteres")]
        public string DNI { get; set; } = string.Empty;

        public string? EstadoCivil { get; set; }
        public string? TipoContrato { get; set; }

        [StringLength(20, MinimumLength = 2, ErrorMessage = "El nombre de usuario debe tener entre 2 y 20 caracteres")]
        public string? Username { get; set; }

        public bool IsActive { get; set; } = true;

        public bool EmailConfirmed { get; set; } = false;

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public DateTime? PasswordChangedDate { get; set; }

        [StringLength(500)]
        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordTokenExpiry { get; set; }

        // Roles
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Empleado"; // Admin, Manager, Empleado, Cliente

        // Relación con RefreshTokens
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        [NotMapped]
        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

        // Relación recursiva (Jefe)
        public int? idEmpleadoJefe { get; set; }
        [ForeignKey("idEmpleadoJefe")]
        public virtual Empleado? Jefe { get; set; }

        // Relaciones muchos a muchos (Navegación)
        public virtual ICollection<CargoEmpleado> CargosEmpleados { get; set; } = new List<CargoEmpleado>();
        public virtual ICollection<EmpleadoBono> EmpleadoBonos { get; set; } = new List<EmpleadoBono>();
    }
}
