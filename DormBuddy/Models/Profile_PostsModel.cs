using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models {
    public class Profile_PostsModel
    {
        public int Id { get; set; }  

        public string? UserId { get; set; } // user creating the post

        [ForeignKey("UserId")] 
        public ApplicationUser? User { get; set; }

        public string? TargetId { get; set; }

        [ForeignKey("TargetId")]
        public ApplicationUser? TargetUser { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Message { get; set; }

        public int Reply_Id { get; set; }

        
    }
}