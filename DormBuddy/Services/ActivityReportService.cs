using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;

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

                return users.Select(user => new UserActivityReport
                {
                    UserId = user.Id,
                    FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                    TotalLogins = user.TotalLogins,
                    LastLoginDate = user.LastLoginDate,
                    GroupsJoined = _context.Groups.Count(g =>
                        g.Members.Any(m => m.Id.ToString() == user.Id)),
                    TasksCreated = _context.Tasks.Count(t =>
                        t.AssignedTo == user.UserName),
                    ExpensesAdded = _context.Expenses.Count(e =>
                        e.UserId.ToString() == user.Id)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateMonthlyActivityReport: {ex.Message}");
                throw;
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
                Console.WriteLine($"Error in GetSystemLogs: {ex.Message}");
                throw;
            }
        }
    }
}
