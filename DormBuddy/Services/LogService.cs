using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;
public class LogService
{
    private readonly DBContext _context;

    public LogService(DBContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(string action, string username, string details)
    {
        var log = new LogModel
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Username = username,
            Details = details
        };

        await _context.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<LogModel>> GetRecentLogsAsync()
    {
        return await _context.Set<LogModel>()
            .OrderByDescending(l => l.Timestamp)
            .Take(50)
            .ToListAsync();
    }
}
