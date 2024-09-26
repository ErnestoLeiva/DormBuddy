using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 



namespace DormBuddy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            return View("~/Views/Account/AdminDashboard.cshtml"); // This should map to AdminDashboard.cshtml in the Views folder
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

            // Handle errors here
            return BadRequest("Error assigning role");
        }
    }
}