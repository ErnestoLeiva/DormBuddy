namespace DormBuddy.Models
{
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
/*
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
        {var users = await _context.ApplicationUsers.AsNoTracking().ToListAsync();
    return users.Select(user => new UserActivityReport
    {
        UserId = user.Id,
        FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
        TotalLogins = user.TotalLogins,
        LastLoginDate = user.LastLoginDate,
        GroupsJoined = _context.Groups.Count(g => g.Members.Any(m => m.Id == user.Id)),
        TasksCreated = _context.Tasks.Count(t => t.AssignedTo == user.UserName),
        ExpensesAdded = _context.Expenses.Count(e => e.UserId == user.Id)
        }).ToList();

 
    }

        public async Task<List<LogModel>> GetSystemLogs()
        {
            return await _context.Logs.AsNoTracking().OrderByDescending(log => log.Timestamp).ToListAsync();

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

*/