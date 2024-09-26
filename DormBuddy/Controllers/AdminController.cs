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

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.Username = User.Identity.Name;
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            return View();
        }

        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Add other admin functionalities here if needed
    }
}
