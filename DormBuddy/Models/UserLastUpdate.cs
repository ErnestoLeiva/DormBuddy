using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models {
    public class UserLastUpdate
    {
        public int Id { get; set; }  
        public string? UserId { get; set; }

        [ForeignKey("UserId")] 
        public ApplicationUser? User { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}