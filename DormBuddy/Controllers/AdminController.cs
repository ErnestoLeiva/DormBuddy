using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using System.Linq;
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

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ActivityReportService activityReportService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _activityReportService = activityReportService;
        }

        // Admin Dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if the user is not logged in
            }

            // Set the username in ViewBag for use in the view
            ViewBag.Username = $"{user.FirstName} {user.LastName}";
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalReports = 10;
            ViewBag.ActiveSessions = 5;

            return View("~/Views/Administration/AdminDashboard.cshtml");
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
    }
}
