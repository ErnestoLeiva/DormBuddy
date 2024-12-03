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
        private readonly LogService _logService;
        private readonly RoleManager<IdentityRole> _roleManager;



        public AdministrationController(UserManager<ApplicationUser> userManager, IReportService reportService, DBContext context, LogService logService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _reportService = reportService;
            _context = context;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _roleManager = roleManager;

        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => View("~/Views/Administration/AdminPanel.cshtml");

       // GET: /Administration/ModeratorDashboard
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ModeratorPanel()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.Username = $"{user.FirstName} {user.LastName}";
                }
                return View("~/Views/Administration/ModeratorPanel.cshtml"); // Ensure the view exists at this path
            }
            else
            {
                return RedirectToAction("AccessDenied");
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View("~/Views/Administration/UserManagement.cshtml", users);
        }

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
            // Fetch logs using LogService
            var logs = await _logService.GetRecentLogsAsync();
            
            if (logs == null || !logs.Any())
            {
                ViewBag.Message = "No logs available.";
            }

            return View("~/Views/Administration/Logs.cshtml", logs);
        }
        public async Task AddLogAsync(string action, string username, string details, string logType = "Info", string description = "No description")
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

        public async Task<List<LogModel>> GetRecentLogsAsync()
        {
            return await _context.Logs
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .ToListAsync();
        }
    
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateSystemSettings(string setting1, bool setting2)
        {
            TempData["Message"] = "Settings updated successfully!";
            return RedirectToAction("SystemSettings");
        }
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            if (roles == null || !roles.Any())
            {
                ViewBag.Message = "No roles available.";
            }
            return View("ManageRoles", roles);
        }

    }
}

