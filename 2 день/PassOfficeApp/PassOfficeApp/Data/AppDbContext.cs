// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using PassOfficeApp.Models;

namespace PassOfficeApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<VisitPurpose> VisitPurposes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PersonalRequest> PersonalRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Установка схемы по умолчанию
            modelBuilder.HasDefaultSchema("kp");

            // Настройка последовательностей для PostgreSQL
            modelBuilder.Entity<User>()
                .Property(u => u.id)
                .UseIdentityColumn();

            modelBuilder.Entity<PersonalRequest>()
                .Property(p => p.id)
                .UseIdentityColumn();
        }
    }
}