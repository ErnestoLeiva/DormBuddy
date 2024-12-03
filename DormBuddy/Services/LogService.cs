using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DormBuddy.Services
{
    public class LogService
    {
        private readonly DBContext _context;

        public LogService(DBContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(string action, string username, string details, string logType = "Info", string? description = null)
        {
            var log = new LogModel
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                Username = username,
                Details = details,
                LogType = logType,
                Description = description

            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        // Update GetRecentLogsAsync to exclude Description
        public async Task<List<LogModel>> GetRecentLogsAsync()
        {
            // Explicitly select only required columns
            return await _context.Logs
                .Select(l => new LogModel
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    Action = l.Action,
                    Username = l.Username,
                    Details = l.Details,
                    LogType = l.LogType,
                    Description = l.Description
                })
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
