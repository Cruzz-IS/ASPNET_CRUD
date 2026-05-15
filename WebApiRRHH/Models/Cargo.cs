using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    [Table("Cargo")]
    public class Cargo : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCargo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        public decimal? SueldoBase { get; set; }

        public virtual ICollection<CargoEmpleado> CargosEmpleados { get; set; } = new List<CargoEmpleado>();

    }
}
