using Microsoft.AspNetCore.Identity;

namespace DormBuddy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int Credits { get; set; } // Assuming you have Credits in your user model

        public ApplicationUser() { }
    }
}

