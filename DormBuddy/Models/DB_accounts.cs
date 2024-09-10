using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models
{
    public class DB_accounts
    {
        [Key]
        public int user_id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public int balance { get; set; }
    }
}