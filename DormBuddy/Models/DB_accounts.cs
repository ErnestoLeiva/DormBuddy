namespace DormBuddy.Models
{
    public class DB_accounts
    {
        public int Id { get; set; } // Primary key
        public string AccountName { get; set; } = string.Empty;// Example property

    }
}
using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models
{
    public class DB_accounts
    {
        [Key]
        public int user_id { get; set; }
        public required string email { get; set; }
        public required string username { get; set; }
        public required string password { get; set; }
        public required string firstname { get; set; }
        public required string lastname { get; set; }
        public required int balance { get; set; }
        public bool IsAdmin { get; set; }  // New property for admin check
    }
}
