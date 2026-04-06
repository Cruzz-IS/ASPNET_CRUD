using Microsoft.EntityFrameworkCore;

namespace WebApiRRHH.Context
{
    public class AppDBContext : DbContext
    {
        public DbSet<Models.User> Users { get; set; }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
    }
}
