using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    [Table("CargoEmpleado")]
    public class CargoEmpleado : BaseEntity
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

        public virtual Empleado Empleado { get; set; } = null!;

        public DateTime? fechaNombramiento { get; set; }
    }
}
