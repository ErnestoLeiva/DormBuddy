using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DormBuddy.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AdministrationController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        
        #region ADMIN/MODERATOR PANEL ACTIONS
        
        // GET: /Administration/AdminDashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPanel()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.Username = $"{user.FirstName} {user.LastName}";
                }
                return View("~/Views/Administration/AdminPanel.cshtml");
            }
            else
            {
                return RedirectToAction("AccessDenied");
            }
        }

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
                return View("~/Views/Administration/ModeratorPanel.cshtml");
            }
            else
            {
                return RedirectToAction("AccessDenied");
            }
        }
        
        #endregion
    }
}
