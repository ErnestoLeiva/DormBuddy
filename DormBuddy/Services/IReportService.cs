using System.Collections.Generic;
using System.Threading.Tasks;
using DormBuddy.Models;

namespace DormBuddy.Services
{
    public interface IReportService
    {
        Task<List<UserActivityReport>> GenerateMonthlyActivityReport();
        Task<List<LogModel>> GetSystemLogs();
    }
}
