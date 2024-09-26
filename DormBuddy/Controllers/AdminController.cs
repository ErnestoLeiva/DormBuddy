using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DormBuddy.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // Add other admin functionalities here
    }
}
