using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema; 

namespace DormBuddy.Models
{
    public class FriendsModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FriendId { get; set; }

        public bool blocked { get; set; }
    }
}
