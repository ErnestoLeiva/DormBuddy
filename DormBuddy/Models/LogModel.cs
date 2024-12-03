namespace DormBuddy.Models
{
    public class LogModel
    {
        public int Id { get; set; } // Primary key
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Username { get; set; }
        public string Details { get; set; }
        public string LogType { get; set; }
        public string? Description { get; set; } 


    }
}
