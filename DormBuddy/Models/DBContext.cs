using Microsoft.EntityFrameworkCore;

namespace DormBuddy.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        // Define DbSet properties for your entities
        public DbSet<DB_accounts> accounts { get; set; }
    }
}
