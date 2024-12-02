using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // For UserManager and Identity types
using DormBuddy.Models;             // For ApplicationUser


namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
         private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to inject UserManager
        public AdministrationController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Administration/AdminPanel
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => View("~/Views/Administration/AdminPanel.cshtml");

        // GET: /Administration/ModeratorPanel
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModeratorPanel() => View("~/Views/Administration/ModeratorPanel.cshtml");
        
        [Authorize(Roles = "Admin")]
        public IActionResult ManageUsers()
        {
            // Get the list of users from UserManager
            var users = _userManager.Users.ToList();
            return View("~/Views/Administration/UserManagement.cshtml", users);
        }


        public IActionResult ModerateContent()
        {
            // Add logic for moderating content
            return View("~/Views/Administration/ModeratorPanel.cshtml");
        }

        public IActionResult SystemSettings()
        {
            // Add logic for system settings
            return View("~/Views/Administration/SystemSettings.cshtml");
        }

        public IActionResult Reports()
        {
            // Add logic for generating reports
            return View("~/Views/Administration/Reports.cshtml");
        }

        public IActionResult SystemLogs()
        {
            // Add logic for viewing system logs
            return View("~/Views/Administration/Logs.cshtml");
        }

        public IActionResult ManageRoles()
        {
            // Add logic for managing roles
            return View("~/Views/Administration/UserManagement.cshtml");
        }
    }
}