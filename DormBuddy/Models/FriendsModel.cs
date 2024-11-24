using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models
{
    public class FriendsModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? FriendId { get; set; }

        public bool blocked { get; set; }
        public bool pending { get; set; }
    }
}
