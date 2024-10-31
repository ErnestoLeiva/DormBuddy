using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DormBuddy.Models
{
    public class DBContext : IdentityDbContext<ApplicationUser>
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
