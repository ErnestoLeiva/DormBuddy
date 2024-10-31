using Microsoft.AspNetCore.Identity;

namespace DormBuddy.Models
{

    public class ApplicationUser : IdentityUser
    {
        public int Credits { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool RememberMe { get; set; }

        // Navigation property for many-to-many relationship with GroupModel
        public List<GroupModel> Groups { get; set; } = new List<GroupModel>();
    }

}

