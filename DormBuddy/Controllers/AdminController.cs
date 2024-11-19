using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using DormBuddy.Services;
using DormBuddy.ViewModels;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DormBuddy.Services;
using System.Text;

namespace DormBuddy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ActivityReportService _activityReportService;
        private readonly DBContext _dbContext; 

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ActivityReportService activityReportService, DBContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _activityReportService = activityReportService;
            _dbContext = dbContext; 

        }

        // Admin Dashboard
        public async Task<IActionResult> AdminDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username = $"{user.FirstName} {user.LastName}";
            var logs = await _activityReportService.GetSystemLogs();
            var monthlyReport = await _activityReportService.GenerateMonthlyActivityReport();
            var users = await _userManager.Users.ToListAsync();

            var model = new AdminDashboardViewModel
            {
                Logs = logs,
                MonthlyReport = monthlyReport,
                Users = users
            };

            return View("~/Views/Administration/AdminDashboard.cshtml", model);
        }
        // GET: /Admin/ExportMonthlyReport (CSV Download)
        [HttpGet]
        public async Task<IActionResult> ExportMonthlyReport()
        {
            var report = await _activityReportService.GenerateMonthlyActivityReport();
            var csv = new StringBuilder();
            csv.AppendLine("User,Total Logins,Last Login,Groups Joined,Tasks Created,Expenses Added");

            foreach (var user in report)
            {
                csv.AppendLine($"{user.FullName},{user.TotalLogins},{user.LastLoginDate},{user.GroupsJoined},{user.TasksCreated},{user.ExpensesAdded}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "MonthlyReport.csv");
        }
        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Method to create the Admin role if it doesn't exist
        public async Task<IActionResult> CreateRole()
        {
            var roleExist = await _roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                // Create the Admin role
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            return Ok("Role created if it did not exist");
        }

        // Method to assign a user to the Admin role
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                return Ok("Role assigned successfully");
            }

            return BadRequest("Error assigning role");
        }

        // GET: /Admin/MonthlyReport (HTML View)
        [HttpGet]
        public async Task<IActionResult> MonthlyReportView()
        {
            var report = await _activityReportService.GenerateMonthlyActivityReport();
            return View("~/Views/Admin/MonthlyReport.cshtml", report);
        }
  

        // View activity logs
        public async Task<IActionResult> Logs()
        {
            var logs = await _activityReportService.GetSystemLogs();
            if (logs == null || !logs.Any())
            {
                Console.WriteLine("No logs found.");
            }
            return View("~/Views/Admin/Logs.cshtml", logs);
        }
        public async Task<IActionResult> SystemLogs()
        {
            var logs = await _dbContext.Logs.ToListAsync();
            if (logs == null || !logs.Any())
            {
                logs = new List<LogModel>(); 
            }
            return View("~/Views/Administration/Logs.cshtml", logs);
        }



    }
}
