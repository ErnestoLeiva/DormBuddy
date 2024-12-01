namespace DormBuddy.Models
{
    public class UserActivityReport
    {
        public int Id { get; set; } 
                public string UserId { get; set; }  // Ensure UserId is defined

        public string FullName { get; set; }
        public int TotalLogins { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int GroupsJoined { get; set; }
        public int TasksCreated { get; set; }
        public int ExpensesAdded { get; set; }
    }
}
