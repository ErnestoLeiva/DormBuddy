using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;

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
                UserId = user.Id,
                FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                TotalLogins = user.TotalLogins,
                LastLoginDate = user.LastLoginDate,
                GroupsJoined = _context.Groups.Count(g =>
                    g.Members.Any(m => m.Id.ToString() == user.Id)), // Convert m.Id to string
                TasksCreated = _context.Tasks.Count(t =>
                    t.AssignedTo == user.UserName), // Ensure AssignedTo is string
                ExpensesAdded = _context.Expenses.Count(e =>
                    e.UserId.ToString() == user.Id) // Convert e.UserId to string
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
