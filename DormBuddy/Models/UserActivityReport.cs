using DormBuddy.Models;

public class UserActivityReport
    {
        public string UserId { get; set; } = string.Empty; // Ensure initialization
        public string FullName { get; set; } = string.Empty; // Ensure initialization
        public int TotalLogins { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int GroupsJoined { get; set; }
        public int TasksCreated { get; set; }
        public int ExpensesAdded { get; set; }
    }