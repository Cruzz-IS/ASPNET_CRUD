using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    [Table("CargoEmpleado")]
    public class CargoEmpleado : BaseEntity
    {
        public int IdCargo { get; set; }
        public virtual Cargo Cargo { get; set; } = null!;
        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; } = null!;
        public DateTime? fechaNombramiento { get; set; }
    }
}
