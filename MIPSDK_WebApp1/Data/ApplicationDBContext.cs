using Microsoft.EntityFrameworkCore;
using MIPSDK_WebApp1.Models.Entities;

namespace MIPSDK_WebApp1.Data
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<DataPolicy> DataPolicies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .Property(e => e.Dob)
                .HasColumnType("date");

            modelBuilder.Entity<Employee>()
                .Property(e => e.HireDate)
                .HasColumnType("date");
        }
    }
}
