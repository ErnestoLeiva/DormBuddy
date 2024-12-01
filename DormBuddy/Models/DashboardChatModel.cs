using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models
{
    public class DashboardChatModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")] 
        public ApplicationUser? User { get; set; }

        // type 1 = global, 2 = dorm, 3 = private message
        public int type { get; set; }

        public DateTime sent_at { get; set; }

        [Column(TypeName = "TEXT")] // Specify text data type
        public string? message { get; set; }
    }
}
