using WebApiRRHH.Models;

namespace WebApiRRHH.Repositories.Interfaces
{
    public interface IEmpleadoRepository
    {
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<Empleado?> GetByEmailAsync(string email);
        Task<Empleado> CreateAsync(Empleado user);
        Task<Empleado> UpdateAsync(Empleado user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeEmpleadoId = null);
    }
}
