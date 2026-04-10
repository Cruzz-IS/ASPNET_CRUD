using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using WebApiRRHH.Models;

namespace WebApiRRHH.Context
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        //public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        //public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la entidad User
            modelBuilder.Entity<User>(entity =>
            {
                // Índice único para el email
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                // Índice para filtros de usuarios activos
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Users_IsActive");

                // Índice para role
                entity.HasIndex(e => e.Role)
                    .HasDatabaseName("IX_Users_Role");

                // Configuración de propiedades
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Employee");

                // Relación con RefreshTokens
                //entity.HasMany(u => u.RefreshTokens)
                //    .WithOne(rt => rt.User)
                //    .HasForeignKey(rt => rt.UserId)
                //    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de RefreshToken
            //modelBuilder.Entity<RefreshToken>(entity =>
            //{
            //    entity.HasIndex(e => e.Token)
            //        .HasDatabaseName("IX_RefreshTokens_Token");

            //    entity.HasIndex(e => e.UserId)
            //        .HasDatabaseName("IX_RefreshTokens_UserId");

            //    entity.HasIndex(e => e.ExpiresAt)
            //        .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

            //    entity.Property(e => e.CreatedAt)
            //        .HasDefaultValueSql("GETUTCDATE()");
            //});

            // Configuración de AuditLog
            //modelBuilder.Entity<AuditLog>(entity =>
            //{
            //    entity.HasIndex(e => e.UserId)
            //        .HasDatabaseName("IX_AuditLogs_UserId");

            //    entity.HasIndex(e => e.Timestamp)
            //        .HasDatabaseName("IX_AuditLogs_Timestamp");

            //    entity.HasIndex(e => e.Action)
            //        .HasDatabaseName("IX_AuditLogs_Action");

            //    entity.Property(e => e.Timestamp)
            //        .HasDefaultValueSql("GETUTCDATE()");
            //});

            // Datos de prueba 
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Password: Admin@123 (hasheado con BCrypt)
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = adminPasswordHash,
                    PhoneNumber = "+504 9999-0000",
                    Role = "Admin",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedDate = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Name = "Juan",
                    Email = "juan.perez@yahoo.com",
                    PasswordHash = userPasswordHash,
                    PhoneNumber = "+504 9999-8888",
                    Role = "Empleado",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedDate = DateTime.UtcNow
                },
                new User
                {
                    Id = 3,
                    Name = "María",
                    Email = "maria.gonzalez@gmail.com",
                    PasswordHash = userPasswordHash,
                    PhoneNumber = "+504 9999-7777",
                    Role = "Cliente",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedDate = DateTime.UtcNow
                }
            );
        }

        //public DbSet<Models.User> Users { get; set; }

        //public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
    }
}
