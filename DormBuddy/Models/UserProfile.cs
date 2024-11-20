using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models
{
    public class UserProfile
    {
        public int Id { get; set; }  
        public string UserId { get; set; }

        [ForeignKey("UserId")] 
        public ApplicationUser User { get; set; }

        /*
        // Account infomration
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        */

        // Basic Profile Info
        public string? Bio { get; set; }
        /*
        public string? BannerImageUrl { get; set; }
        */
        public string? ProfileImageUrl { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Social Media Links
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? InstagramUrl { get; set; }

        // Preferences
        public string? Preferred_Language { get; set; }
        public bool ReceiveEmailNotifications { get; set; }
        public bool ProfileVisibleToPublic { get; set; }

        // Work/Education
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? SchoolName { get; set; }


        // Status/Activity
        public DateTime LastLogin { get; set; }
        public string? AccountStatus { get; set; }
    }
}
