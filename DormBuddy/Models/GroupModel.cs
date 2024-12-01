using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models
{
    public class GroupModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int MaxMembers { get; set; } = 4;

        public int TotalMembers { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        [Required]
        public string InvitationCode { get; set; }

        public ICollection<GroupMemberModel> Members { get; set; } = new List<GroupMemberModel>();
    }
}