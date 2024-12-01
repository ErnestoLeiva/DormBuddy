using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormBuddy.Models
{
    public class GroupMemberModel
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int GroupId { get; set; }

        public bool IsAdmin { get; set; } = false;

        [ForeignKey(nameof(GroupId))]
        public GroupModel Group { get; set; }
    }

}
