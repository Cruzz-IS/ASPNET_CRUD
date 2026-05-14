using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    [Table("CargoEmpleado")]
    public class CargoEmpleado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCargo { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEmpleado { get; set; }

        [ForeignKey("IdCargo")]

        public virtual Cargo Cargo { get; set; } = null!;

        [ForeignKey("IdEmpleado")]

        public virtual User User { get; set; } = null!;

        // Auditoría
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
