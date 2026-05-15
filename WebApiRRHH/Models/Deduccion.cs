using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRRHH.Models
{
    public class Deduccion : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdDeduccion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal valor { get; set; }

        public int idTipoDeduccion { get; set; }

        public virtual TipoDeduccion TipoDeduccion { get; set; }

    }
}
