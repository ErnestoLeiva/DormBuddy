using System.Diagnostics;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DormBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DormBuddy.Models.IEmailSender _emailSender;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DormBuddy.Models.IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        #region ACCOUNT FORMS

        // GET: /Account/AccountForms
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

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View("AccountForms");
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);

                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("Activation", "Account", new { userId = user.Id, token }, Request.Scheme);
                        
                        if (!string.IsNullOrEmpty(confirmationLink))
                        {
                            await _emailSender.SendActivationEmail(user, confirmationLink);
                            TempData["message"] = "Email confirmation has been sent! Please check your inbox.";
                        }
                        else
                        {
                            Console.WriteLine("Confirmation link is null! Activation email not sent to user!");
                        }

                        ViewBag.ErrorMessage = "Your email has not been confirmed. Please check your email for confirmation. If needed, try logging in again to resend the email.";
                        return View("AccountForms");
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard");
                    }

                    ViewBag.ErrorMessage = "Invalid credentials, try again!";
                    return View("AccountForms");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Username/Email entered: User does not exist.");
                    ViewBag.ErrorMessage = "Invalid Username/Email entered: User does not exist.";
                    return View("AccountForms");
                }
            }

            ViewBag.ErrorMessage = "Invalid credentials, try again!";
            return View("AccountForms");
        }

        #endregion

        #region SIGN UP

        // GET: /Account/Signup
        public IActionResult Signup()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View("AccountForms");
        }

        // POST: /Account/Signup
        [HttpPost]
        public async Task<IActionResult> Signup(string email, string username, string password, string reenterpassword, string firstname, string lastname)
        {
            if (ModelState.IsValid)
            {
                // Check if passwords match
                if (password != reenterpassword)
                {
                    ModelState.AddModelError(string.Empty, "Passwords do not match.");
                    ViewBag.ErrorMessage = "Passwords do not match!";
                    return View("AccountForms");
                }

                // Check if username exists
                var existingUserByName = await _userManager.FindByNameAsync(username);
                if (existingUserByName != null)
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken.");
                    ViewBag.ErrorMessage = "Username is already taken!";
                    return View("AccountForms");
                }

                // Check if email exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already registered.");
                    ViewBag.ErrorMessage = "Email is already registered!";
                    return View("AccountForms");
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
                    return View("AccountForms");
                }

                // Create user
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
                    // Assign "User" role to the new account
                    await _userManager.AddToRoleAsync(user, "User");
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("Activation", "Account", new { userId = user.Id, token }, Request.Scheme);
                    if (!string.IsNullOrEmpty(confirmationLink))
                        await _emailSender.SendActivationEmail(user, confirmationLink);
                    else
                        Console.WriteLine("Confirmation link is null! Activation email not sent to user!");
                    TempData["message"] = "Email verification has been sent!";
                    return View("AccountForms");
                }

                // Log errors if user creation failed
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("AccountForms");
        }

        #endregion

        #region ACC. CONFIRMATION
        [HttpGet]
        public async Task<IActionResult> Activation(string userId, string token) {
            if (userId == null || token == null) {
                ViewBag.ErrorMessage = "Invalid email confirmation!";
                return RedirectToAction("AccountForms");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                Console.WriteLine("User not found with id {userId}");
                ViewBag.ErrorMessage = "User not found";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) {
                // email confirmed successfully, set activation status and send user to dashboard
                TempData["message"] = "Email confirmation is successful!";
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard");
            } else {
                // not confirmed
                ViewBag.ErrorMessage = "Email confirmation failure!";
                return RedirectToAction("AccountForms");
            }
        }
        #endregion

        #region LOGOUT

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }
            return RedirectToAction("Login");
        }

        #endregion

        #region DASHBOARD

        // GET: /Account/Dashboard
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

        #region ACCESS DENIED

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region ERROR HANDLING

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion
    }
}
