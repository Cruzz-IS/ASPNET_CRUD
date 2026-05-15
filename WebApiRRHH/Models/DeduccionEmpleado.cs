namespace WebApiRRHH.Models
{
    public class DeduccionEmpleado : BaseEntity 
    {
        public int IdDeduccion { get; set; }

        public virtual Deduccion Deduccion { get; set; } = null!;


        public int IdEmpleado { get; set; }

        public virtual Empleado Empleado { get; set; } = null!;

        public int IdPlanilla { get; set; }

        public virtual Planilla Planilla { get; set; } = null!;

        public DateTime Fecha { get; set; }

        public string? EstadoDeduccion  { get; set; }
    }
}
