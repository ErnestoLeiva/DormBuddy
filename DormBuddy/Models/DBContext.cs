using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DormBuddy.Models
{

    public class ApplicationUser : IdentityUser
    {
        public int Credits { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool RememberMe { get; set; }

    }

    public class DBContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        // DBSet for persistent tasks feature - Ernesto Leiva 10/04/2024
        public DbSet<TaskModel> Tasks { get; set; }
        
        // DBSet for persistent expenses feature - Ernesto Leiva 10/27/2024
        public DbSet<ExpenseModel> Expenses { get; set; }

        // DBSet for persistent peer-lending feature - Ernesto Leiva 11/04/2024
        public DbSet<PeerLendingModel> PeerLendings {get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<UserLastUpdate> UserLastUpdate { get; set; } // saves the last time the user updated the browser to find online status

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseMySql("Server=shportfolio.net;Database=myportfolio_dormbuddy;User=myportfolio;Password=65eyqYcPHv;", 
                    new MySqlServerVersion(new Version(8, 0, 2)))
                    .EnableSensitiveDataLogging();
            }
                
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure the maximum length is set for all string properties
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    {
                        // Apply a default maximum length to avoid key length issues
                        property.SetMaxLength(160);
                    }
                }
            }

            // Apply specific maximum length configurations for Identity tables
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(m => m.Email).HasMaxLength(160);
                entity.Property(m => m.NormalizedEmail).HasMaxLength(160);
                entity.Property(m => m.UserName).HasMaxLength(160);
                entity.Property(m => m.NormalizedUserName).HasMaxLength(160);
                entity.Property(m => m.FirstName).HasMaxLength(160);
                entity.Property(m => m.LastName).HasMaxLength(160);

            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(m => m.Name).HasMaxLength(160);
                entity.Property(m => m.NormalizedName).HasMaxLength(160);
            });

            // Specific configuration for IdentityUserLogin
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(e => e.LoginProvider).HasMaxLength(160);
                entity.Property(e => e.ProviderKey).HasMaxLength(160);
                entity.Property(e => e.ProviderDisplayName).HasMaxLength(160);
            });

            // Tasks configuratiobns for the requirments of string lengths
            builder.Entity<TaskModel>(entity =>
            {
                entity.Property(t => t.TaskName).HasMaxLength(160).IsRequired();
                entity.Property(t => t.AssignedTo).HasMaxLength(160).IsRequired();
            });
        }


    }
}
