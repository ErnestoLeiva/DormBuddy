using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DormBuddy.Services
{
    public class ActivityReportService
    {
        private readonly DBContext _context;

        public ActivityReportService(DBContext context)
        {
            _context = context;
        }

        public async Task<List<UserActivityReport>> GenerateMonthlyActivityReport()
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

            // Fetch groups and evaluate in memory
            var groups = await _context.Groups
                .Include(g => g.Members)
                .AsNoTracking()
                .ToListAsync();

            var report = await _context.ApplicationUsers
                .AsNoTracking()
                .ToListAsync();

            var userReports = report.Select(user => new UserActivityReport
            {
                UserId = user.Id,
                FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                TotalLogins = user.TotalLogins,
                LastLoginDate = user.LastLoginDate,
                GroupsJoined = groups.Count(g => g.Members.Any(m => m.Id == user.Id)), // Evaluate memberships in memory
                TasksCreated = _context.Tasks.Count(t => t.AssignedTo == user.UserName),
                ExpensesAdded = _context.Expenses.Count(e => e.UserId == user.Id)
            }).ToList();

            return userReports;
        }
    }

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
}
