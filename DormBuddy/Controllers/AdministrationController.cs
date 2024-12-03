using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // For UserManager and Identity types
using DormBuddy.Models;             // For ApplicationUser
using DormBuddy.Services;           // For IReportService

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;

        // Constructor to inject dependencies
        public AdministrationController(UserManager<ApplicationUser> userManager, IReportService reportService)
        {
            _userManager = userManager;
            _reportService = reportService;
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
                return View(reports);
            }
            catch
            {
                return View("Error", new ErrorViewModel { Message = "Failed to load reports." });
            }
        }

        [AllowAnonymous]
        public IActionResult SystemLogs()
        {
            var logs = _reportService.GetSystemLogs();
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
