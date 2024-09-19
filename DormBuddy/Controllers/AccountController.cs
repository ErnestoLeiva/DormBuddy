using System.Diagnostics;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DormBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult AccountForms()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
            return RedirectToAction("Dashboard", "Account");
            }
            return View();
        }
        #region LOGIN HANDLING
        public IActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Account");
            }
            return View("AccountForms");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard", "Account");
                    } else {
                        ViewBag.ErrorMessage = "Invalid credentials, try again!";
                        return View("AccountForms");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Username/Email entered: User does not exist.");
                ViewBag.ErrorMessage = "Invalid Username/Email entered: User does not exist.";
            }
            return View();
        }

        #endregion
        
        #region SIGN UP/LOG OUT HANDLING
        public IActionResult Signup()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Account");
            }
            return View("AccountForms");
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string username, string password, string reenterpassword, string firstname, string lastname)
        {
            if (ModelState.IsValid)
            {
                // Check if the passwords match
                if (password != reenterpassword)
                {
                    ModelState.AddModelError(string.Empty, "Passwords do not match.");
                    ViewBag.ErrorMessage = "Passwords do not match!";
                    return View();
                }

                // Check if a user with the same username already exists
                var existingUserByName = await _userManager.FindByNameAsync(username);
                if (existingUserByName != null)
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken.");
                    ViewBag.ErrorMessage = "Username is already taken!";
                    return View();
                }

                // Check if a user with the same email already exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already registered.");
                    ViewBag.ErrorMessage = "Email is already registered!";
                    return View();
                }

                // Validate password
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, null, password);
                if (!passwordValidationResult.Succeeded)
                {
                    foreach (var error in passwordValidationResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.ErrorMessage = "Password does not meet the requirements!";
                    return View();
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
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Account");
                }
                else
                {
                    // Log detailed error information
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View();
        }

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
        // GET: /Account/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.Username = $"{user.FirstName} {user.LastName}";
                }
                return View();
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Tasks
        public IActionResult Tasks()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Tasks.cshtml");
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Expenses
        public IActionResult Expenses()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Expenses.cshtml");
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Lending
        public IActionResult Lending()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Lending.cshtml");
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Notifications
        public IActionResult Notifications()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Notifications.cshtml");
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Settings
        public IActionResult Settings()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Settings.cshtml");
            }
            return RedirectToAction("Login");
        }

        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
