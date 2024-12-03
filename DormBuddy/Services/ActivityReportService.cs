using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;

namespace DormBuddy.Services
{
    public class ActivityReportService : IReportService
    {
        private readonly DBContext _context;

        public ActivityReportService(DBContext context)
        {
            _context = context;
        }

       public async Task<List<UserActivityReport>> GenerateMonthlyActivityReport()
{
    try
    {
        var users = await _context.ApplicationUsers.AsNoTracking().ToListAsync();
        
        if (users == null || !users.Any())
        {
            Console.WriteLine("No users found in the database.");
            return new List<UserActivityReport>();
        }

        Console.WriteLine($"Found {users.Count} users.");
        
        var reports = users.Select(user => new UserActivityReport
        {
            UserId = user.Id,
            FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
            TotalLogins = user.TotalLogins,
            LastLoginDate = user.LastLoginDate ?? DateTime.MinValue,
            GroupsJoined = _context.Groups.Count(g =>
                g.Members != null && g.Members.Any(m => m.Id.ToString() == user.Id)),
            TasksCreated = _context.Tasks.Count(t => t.AssignedTo == user.UserName),
            ExpensesAdded = _context.Expenses.Count(e => e.UserId.ToString() == user.Id)
        }).ToList();

        Console.WriteLine($"Generated {reports.Count} activity reports.");
        return reports;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating reports: {ex.Message}");
        return new List<UserActivityReport>();
    }
}


        public async Task<List<LogModel>> GetSystemLogs()
        {
            try
            {
                return await _context.Logs
                    .OrderByDescending(log => log.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Error fetching system logs: {ex.Message}");
                return new List<LogModel>();
            }
        }
    }
}
