namespace WebApiRRHH.Models
{
    public class EmpleadoBono : BaseEntity
    {
        public int Empleado_idEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; } = null!;

        public int Bono_idBono { get; set; }
        public virtual Bono Bono { get; set; } = null!;

        public int Planilla_idPlanilla { get; set; }
        public virtual Planilla Planilla { get; set; } = null!;

        public string? Estado { get; set; }
    }
}
