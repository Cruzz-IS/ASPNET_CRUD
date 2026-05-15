using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    public class Anticipo : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAnticipo { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Monto { get; set; } 
        public string? Estado { get; set; }
        public int Empleado_idEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; } = null!;
        public int Planilla_idPlanilla { get; set; }
        public virtual Planilla Planilla { get; set; } = null!;
    }
}
