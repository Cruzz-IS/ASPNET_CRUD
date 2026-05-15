using Microsoft.EntityFrameworkCore;
using WebApiRRHH.Context;
using WebApiRRHH.Models;
using WebApiRRHH.Repositories.Interfaces;

namespace WebApiRRHH.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly AppDBContext _context;
        private readonly ILogger<EmpleadoRepository> _logger;

        public EmpleadoRepository(AppDBContext context, ILogger<EmpleadoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            try
            {
                return await _context.Empleados!
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Empleados!.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {EmpleadoId}", id);
                throw;
            }
        }

        public async Task<Empleado?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Empleados!
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con email {Email}", email);
                throw;
            }
        }

        public async Task<Empleado> CreateAsync(Empleado empleado)
        {
            try
            {
                empleado.CreatedAt = DateTime.UtcNow;
                _context.Empleados!.Add(empleado);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario creado exitosamente: {Email}", empleado.Email);
                return empleado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        public async Task<Empleado> UpdateAsync(Empleado empleado)
        {
            try
            {
                empleado.UpdatedAt = DateTime.UtcNow;
                _context.Entry(empleado).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario actualizado: ID {EmpleadoId}", empleado.Id);
                return empleado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {EmpleadoId}", empleado.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var empleado = await GetByIdAsync(id);
                if (empleado == null)
                    return false;

                empleado.IsActive = false;
                empleado.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario desactivado: ID {EmpleadoId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {EmpleadoId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empleados!.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeEmpleadoId = null)
        {
            var query = _context.Empleados!.Where(u => u.Email.ToLower() == email.ToLower());

            if (excludeEmpleadoId.HasValue)
            {
                query = query.Where(u => u.Id != excludeEmpleadoId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
