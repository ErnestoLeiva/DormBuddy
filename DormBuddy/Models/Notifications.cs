using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models {
    public class Notifications
    {
        public int Id { get; set; }  
        public string? UserId { get; set; }

        [ForeignKey("UserId")] 
        public ApplicationUser? User { get; set; }

        public int MessageType { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? Message { get; set; }
    }
}