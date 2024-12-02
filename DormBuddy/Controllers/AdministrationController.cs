using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        // GET: /Administration/AdminPanel
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => View("~/Views/Administration/AdminPanel.cshtml");

        // GET: /Administration/ModeratorPanel
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModeratorPanel() => View("~/Views/Administration/ModeratorPanel.cshtml");

        public IActionResult ManageUsers()
        {
            // Add logic for managing users
            return View("~/Views/Administration/UserManagement.cshtml");
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