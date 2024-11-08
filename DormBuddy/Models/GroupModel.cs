using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models
{
    public class GroupModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "GroupName is required.")]
        public string GroupName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty; // UserId of the group creator

        // Navigation property to hold the list of users in the group
      //  public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

    }
}