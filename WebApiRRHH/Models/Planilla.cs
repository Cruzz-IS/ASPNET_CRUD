using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    public class Planilla
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

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
