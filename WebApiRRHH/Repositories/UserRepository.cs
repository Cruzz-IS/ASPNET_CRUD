using Microsoft.EntityFrameworkCore;
using WebApiRRHH.Context;
using WebApiRRHH.Models;
using WebApiRRHH.Repositories.Interfaces;

namespace WebApiRRHH.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDBContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDBContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users!
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

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Users!.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Users!
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con email {Email}", email);
                throw;
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                _context.Users!.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario creado exitosamente: {Email}", user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario actualizado: ID {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var user = await GetByIdAsync(id);
                if (user == null)
                    return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario desactivado: ID {UserId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users!.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var query = _context.Users!.Where(u => u.Email.ToLower() == email.ToLower());

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
