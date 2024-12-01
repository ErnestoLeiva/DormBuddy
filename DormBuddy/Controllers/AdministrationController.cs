using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly DBContext _dbContext;

        public AdministrationController(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => View("~/Views/Administration/AdminPanel.cshtml");

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModeratorPanel() => View("~/Views/Administration/ModeratorPanel.cshtml");

        public IActionResult SystemLogs() => View("~/Views/Administration/Logs.cshtml");

        public IActionResult MonthlyReport() => View("~/Views/Administration/MonthlyReport.cshtml");

        public IActionResult Reports() => View("~/Views/Administration/Reports.cshtml");

        public IActionResult UserManagement() => View("~/Views/Administration/UserManagement.cshtml");
    }
}
