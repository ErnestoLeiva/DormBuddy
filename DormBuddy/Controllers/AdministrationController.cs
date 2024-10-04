using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        // GET: /Administration/AdminDashboard
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Administration/AdminPanel.cshtml") : RedirectToAction("AccessDenied");
        
        // GET: /Administration/ModeratorDashboard
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ModeratorPanel() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Administration/ModeratorPanel.cshtml") : RedirectToAction("AccessDenied");

    }
}
