namespace WebApiRRHH.Models
{
    public class DeduccionEmpleado : BaseEntity 
    {
        public int IdDeduccion { get; set; }

        public int IdEmpleado { get; set; }

        public int IdPlanilla { get; set; }

        public DateTime Fecha { get; set; }

        public string? EstadoDeduccion  { get; set; }
    }
}
