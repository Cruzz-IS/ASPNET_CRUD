using Microsoft.EntityFrameworkCore;

namespace WebApiRRHH.Context
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }
    }
}
