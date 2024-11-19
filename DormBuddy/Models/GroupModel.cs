namespace DormBuddy.Models
{
    public class GroupModel
    {
        public int Id { get; set; }

        // Use `required` (C# 11+) or provide default values for non-nullable properties
        public required string GroupName { get; set; }
        public required string CreatedBy { get; set; }

        // Navigation property for many-to-many relationship with ApplicationUser
        public ICollection<ApplicationUser>? Members { get; set; }
    }
}
