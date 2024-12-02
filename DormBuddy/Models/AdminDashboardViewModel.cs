using DormBuddy.Models; 
using System.Collections.Generic;

namespace DormBuddy.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<UserActivityReport> MonthlyReport { get; set; }
        public List<LogModel> Logs { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}
