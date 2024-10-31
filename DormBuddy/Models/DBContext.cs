using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DormBuddy.Models;

namespace DormBuddy.Models
{
    public class DBContext : IdentityDbContext<ApplicationUser>

    public class ApplicationUser : IdentityUser
    {
        public int Credits { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool RememberMe { get; set; }

    }

    public class DBContext : IdentityIdentityDbContext<ApplicationUser, IdentityRole, string><ApplicationUser, IdentityRole, string>
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        // Define DbSet properties for your entities
        public DbSet<DB_accounts> Accounts { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<GroupModel> Groups { get; set; }
        public DbSet<ExpenseModel> Expenses { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call the base method to configure Identity
            
            modelBuilder.Entity<GroupModel>()
                .HasMany(g => g.Users)
                .WithMany(u => u.Groups)
                .UsingEntity(j => j.ToTable("UserGroups"));
            // Additional model configurations can be added here if needed
        }
    }
}
