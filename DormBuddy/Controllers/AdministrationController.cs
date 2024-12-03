using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using DormBuddy.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;
        private readonly DBContext _context;

        public AdministrationController(UserManager<ApplicationUser> userManager, IReportService reportService, DBContext context)
        {
            _userManager = userManager;
            _reportService = reportService;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => View("~/Views/Administration/AdminPanel.cshtml");

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModeratorPanel() => View("~/Views/Administration/ModeratorPanel.cshtml");

        [Authorize(Roles = "Admin")]
        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View("~/Views/Administration/UserManagement.cshtml", users);
        }

        public IActionResult ModerateContent() => View("~/Views/Administration/ModerateContent.cshtml");

        public IActionResult SystemSettings() => View("~/Views/Administration/SystemSettings.cshtml");

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var reports = await _reportService.GenerateMonthlyActivityReport();
                if (!reports.Any())
                {
                    ViewBag.Message = "No activity reports found.";
                }
                return View("~/Views/Administration/Reports.cshtml", reports);
            }
            catch
            {
                var errorModel = new ErrorViewModel
                {
                    Message = "Failed to load reports.",
                    RequestId = HttpContext.TraceIdentifier
                };
                return View("Error", errorModel);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> SystemLogs()
        {
            var logs = await _reportService.GetSystemLogs();
            return View("~/Views/Administration/Logs.cshtml", logs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateSystemSettings(string setting1, bool setting2)
        {
            TempData["Message"] = "Settings updated successfully!";
            return RedirectToAction("SystemSettings");
        }

        public IActionResult ManageRoles() => View("~/Views/Administration/UserManagement.cshtml");
    }
}

/*
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


*/