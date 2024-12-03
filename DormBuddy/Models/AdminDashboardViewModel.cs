using DormBuddy.Models; 
using System.Collections.Generic;

namespace DormBuddy.ViewModels
{
    public class AdminDashboardViewModel
    {
         public List<UserActivityReport> MonthlyReport { get; set; } = new List<UserActivityReport>();
        public List<LogModel> Logs { get; set; } = new List<LogModel>();
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
   
    }


}
