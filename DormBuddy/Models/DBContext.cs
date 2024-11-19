using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DormBuddy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int Credits { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool RememberMe { get; set; }
        public string? TimeZone { get; set; }
        public int TotalLogins { get; set; }  // Tracks the number of logins
        public DateTime? LastLoginDate { get; set; }  // Tracks the last login time

        // Navigation property for many-to-many relationship with GroupModel
        public ICollection<GroupModel>? Groups { get; set; }
    }

    public class DBContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        // DBSet for persistent features
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<ExpenseModel> Expenses { get; set; }
        public DbSet<PeerLendingModel> PeerLendings { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<GroupModel> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=shportfolio.net;Database=myportfolio_dormbuddy;User=myportfolio;Password=65eyqYcPHv;",
                    new MySqlServerVersion(new Version(8, 0, 2)));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-Many Relationship: ApplicationUser <-> GroupModel
            builder.Entity<GroupModel>()
                .HasMany(g => g.Members)
                .WithMany(u => u.Groups)
                .UsingEntity(j => j.ToTable("UserGroups"));

            // Configure default string lengths and other properties
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                    {
                        property.SetMaxLength(160);
                    }
                }
            }

            // Configure Identity tables
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

            // Configure IdentityUserLogin
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(e => e.LoginProvider).HasMaxLength(160);
                entity.Property(e => e.ProviderKey).HasMaxLength(160);
                entity.Property(e => e.ProviderDisplayName).HasMaxLength(160);
            });

            // Task configuration
            builder.Entity<TaskModel>(entity =>
            {
                entity.Property(t => t.TaskName).HasMaxLength(160).IsRequired();
                entity.Property(t => t.AssignedTo).HasMaxLength(160).IsRequired();
            });
        }
    }
}
