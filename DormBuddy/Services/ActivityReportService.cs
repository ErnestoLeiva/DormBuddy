using DormBuddy.Models;
using DormBuddy.Services;

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
            var users = await _context.ApplicationUsers.AsNoTracking().ToListAsync();

            return users.Select(user => new UserActivityReport
            {
                UserId = user.Id,  // Ensure UserId exists and is a string
                FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                TotalLogins = user.TotalLogins,
                LastLoginDate = user.LastLoginDate,
                GroupsJoined = _context.Groups.Count(g =>
                    g.Members.Any(m => m.UserId == user.Id)), // Ensure g.Members and m.UserId are properly mapped
                TasksCreated = _context.Tasks.Count(t =>
                    t.AssignedTo == user.UserName), // Ensure AssignedTo is string
                ExpensesAdded = _context.Expenses.Count(e =>
                    e.UserId == user.Id) // Ensure e.UserId is string
            }).ToList();
        }

        public async Task<List<LogModel>> GetSystemLogs()
        {
            return await _context.Logs
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

    }
}
