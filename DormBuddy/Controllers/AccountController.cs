using System.Diagnostics;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DormBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DBContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        #region ACCOUNT FORMS

        public IActionResult AccountForms()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        #endregion

        #region LOGIN

        public IActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View("AccountForms");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            bool isPersistent = false; // Default value, modify based on your logic

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard", "Account");
                    }
                    ViewBag.ErrorMessage = "Invalid credentials, try again!";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Username/Email entered: User does not exist.");
                }
            }
            return View("AccountForms");
        }

        #endregion

        #region SIGN UP/LOG OUT HANDLING

        public IActionResult Signup()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View("AccountForms");
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string username, string password, string reenterpassword, string firstname, string lastname)
        {
    if (ModelState.IsValid)
    {
        if (password != reenterpassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            ViewBag.ErrorMessage = "Passwords do not match!";
            return View("AccountForms");
        }

        var existingUserByName = await _userManager.FindByNameAsync(username);
        if (existingUserByName != null)
        {
            ModelState.AddModelError(string.Empty, "Username is already taken.");
            ViewBag.ErrorMessage = "Username is already taken!";
            return View("AccountForms");
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            ModelState.AddModelError(string.Empty, "Email is already registered.");
            ViewBag.ErrorMessage = "Email is already registered!";
            return View("AccountForms");
        }

        var passwordValidator = new PasswordValidator<ApplicationUser>();
        var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, null, password);
        if (!passwordValidationResult.Succeeded)
        {
            foreach (var error in passwordValidationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.ErrorMessage = "Password does not meet the requirements!";
            return View("AccountForms");
        }

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            FirstName = firstname,
            LastName = lastname,
            Credits = 0
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false); // Corrected line
            return RedirectToAction("Dashboard");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
    return View("AccountForms");
        }

        #endregion

        #region LOGOUT

        public async Task<IActionResult> Logout()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }
            return RedirectToAction("Login");
        }

        #endregion

        #region DASHBOARD HANDLING

        public async Task<IActionResult> Dashboard()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    ViewBag.Username = $"{user.FirstName} {user.LastName}";
                    ViewBag.UserRoles = string.Join(", ", roles);
                }
                return View();
            }
            return RedirectToAction("Login");
        }

        #endregion

        #region DASHBOARD SECTIONS

        public IActionResult Tasks() => CheckAuthenticationAndRedirect("~/Views/Account/Dashboard/Tasks.cshtml");

        public IActionResult Expenses() => CheckAuthenticationAndRedirect("~/Views/Account/Dashboard/Expenses.cshtml");

        public IActionResult Lending() => CheckAuthenticationAndRedirect("~/Views/Account/Dashboard/Lending.cshtml");

        public IActionResult Notifications() => CheckAuthenticationAndRedirect("~/Views/Account/Dashboard/Notifications.cshtml");

        public IActionResult Settings() => CheckAuthenticationAndRedirect("~/Views/Account/Dashboard/Settings.cshtml");

        private IActionResult CheckAuthenticationAndRedirect(string view)
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View(view);
            }
            return RedirectToAction("Login");
        }

        #endregion

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
