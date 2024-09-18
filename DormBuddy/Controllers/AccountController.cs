using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DormBuddy.Controllers
{
    public class AccountController : BaseController
    {

        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        private readonly TimeZoneService _timeZoneService;

        private readonly IConfiguration _configuration;

        private readonly DBContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly DBContext _context;
        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            TimeZoneService timeZoneService,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            DBContext context) : base(userManager, signInManager, context, logger, memoryCache, timeZoneService, configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = _context;
        }

        #region LOGIN HANDLING
        public IActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Account");
            } ///comment
            return View();
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
                        return View();
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string username, string password, string reenterpassword, string firstname, string lastname)
        {
            if (!ModelState.IsValid)
                return View("AccountForms");

            if (password != reenterpassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                ViewBag.ErrorMessage = "Passwords do not match!";
                return View("AccountForms");
            }

            if (await _userManager.FindByNameAsync(username) != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                ViewBag.ErrorMessage = "Username is already taken!";
                return View("AccountForms");
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                ViewBag.ErrorMessage = "Email is already registered!";
                return View("AccountForms");
            }

            var passwordValidator = new PasswordValidator<ApplicationUser>();
            var placeholder = new ApplicationUser();
            var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, placeholder, password);

            if (!passwordValidationResult.Succeeded)
            {
                foreach (var error in passwordValidationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.ErrorMessage = "Password does not meet the requirements!";
                return View("AccountForms");
            }
        #region SIGN UP
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
        public IActionResult Dashboard()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel();

            ViewData["UserId"] = userId;
            ViewData["Token"] = token;

            return View("~/Views/Account/Password/ResetPassword.cshtml", model);
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string userId, string token, ResetPasswordViewModel model) {
            
            if (ModelState.IsValid) {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null) {
                    return RedirectToAction("ForgotPassword");
                }

                if (model.Password != model.ConfirmPassword) {
                    ViewBag.ErrorMessage = "The passwords do not match!";
                    Console.WriteLine("Passwords do not match for password reset!");
                    return View("~/Views/Account/Password/ResetPassword.cshtml", model);
                }


                if (!string.IsNullOrEmpty(model.Password)) {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token??"null", model.Password);

                    if (result.Succeeded)
                    {
                        TempData["message"] = "Password has been reset!";
                        Console.WriteLine("Account password has been reset for " + user.Email);
                        return RedirectToAction("AccountForms");
                    } else {
                        Console.WriteLine("Password reset has failed for " + user.Email);


                        foreach (var er in result.Errors)
                        {
                            Console.WriteLine(er.Description);
                        }

                        return View("~/Views/Account/Password/ResetPassword.cshtml", model);
                    }

                    
                }

            } else {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            return View("~/Views/Account/Password/ResetPassword.cshtml", model);
        }

        #endregion

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

        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #region DASHBOARD SECTIONS

        public IActionResult Tasks() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Tasks.cshtml") : RedirectToAction("AccountForms");

        public IActionResult Expenses() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Expenses.cshtml") : RedirectToAction("AccountForms");

        public IActionResult Lending() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Lending.cshtml") : RedirectToAction("AccountForms");

        //public IActionResult Notifications() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Notifications.cshtml", new List<Notifications>()) : RedirectToAction("AccountForms");

public IActionResult Settings()
{
    if (User?.Identity?.IsAuthenticated == true)
    {
        // Retrieve 'page' query parameter, default to empty string if not provided
        string loadPage = Request.Query["page"].ToString();

        // Define allowed pages
        var allowedPages = new HashSet<string> { "AccountSettings", "GeneralSettings", "PrivacySettings", "ProfileSettings" };

        // Validate the 'page' parameter
        if (!string.IsNullOrEmpty(loadPage) && !allowedPages.Contains(loadPage))
        {
            // Redirect to a default page or return a friendly error message if invalid
            return RedirectToAction(nameof(Settings), new { page = "GeneralSettings" });
        }

        // Pass the page to the view
        ViewBag.LoadPage = string.IsNullOrEmpty(loadPage) ? "GeneralSettings" : loadPage;
        return View("~/Views/Account/Dashboard/Settings.cshtml");
    }

    // Redirect unauthenticated users to the account forms
    return RedirectToAction("AccountForms");
}



        public async Task<IActionResult> Profile()
        {
            try
            {
                // Ensure the user is authenticated
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return RedirectToAction("Login");
                }

                // Get the username from query parameter or fallback to User.Identity.Name
                string get_username = Request.Query["username"].ToString() ?? "";
                get_username = string.IsNullOrEmpty(get_username) ? User?.Identity?.Name : get_username;

                // If username is still null or empty, redirect to the dashboard
                if (string.IsNullOrEmpty(get_username))
                {
                    return RedirectToAction("Dashboard");
                }

                var profile = await GetUserInformation(get_username);

                var u = profile.User;
                if (u == null)
                {
                    return RedirectToAction("Dashboard");
                }

                // If profile is null, redirect to the dashboard
                if (profile == null)
                {
                    return RedirectToAction("Dashboard");
                }

                var bannerImage = _configuration["Profile:Default_BannerImage"];
                if (string.IsNullOrEmpty(profile.BannerImageUrl))
                {
                    profile.BannerImageUrl = bannerImage;
                }

                var profileImage = _configuration["Profile:Default_ProfileImage"];
                if (string.IsNullOrEmpty(profile.ProfileImageUrl))
                {
                    profile.ProfileImageUrl = profileImage;
                }

                var adjustedLastLogin = getCurrentTimeFromUTC(profile.LastLogin);
                ViewData["AdjustedLastLogin"] = adjustedLastLogin;

                // Profile Online Status Check
                ViewData["profile_online_status"] = "Offline";
                var getLastUpdate = await getUserLastUpdate(profile.User);
                if (getLastUpdate?.LastUpdate is DateTime lastUpdate &&
                    (DateTime.UtcNow - lastUpdate).TotalSeconds < 300)
                {
                    ViewData["profile_online_status"] = "Online";
                }

                if (profile.User.UserName != User?.Identity?.Name) {
                    var fstatus = await FriendshipStatus(profile.User);
                    ViewData["FriendshipStatus"] = fstatus;
                }

                ViewData["FriendCount"] = await GetFriendCount(get_username);

                ViewData["Friends"] = await GetAllFriends(get_username);

                return View("~/Views/Account/Dashboard/Profile.cshtml", profile);
            }
            catch (Exception ex)
            {
                // Log the exception and display an error page
                _logger.LogError($"Error in Profile action: {ex.Message}");
                return View("Error");  // Show a generic error page or message
            }
        }


        public async Task<IActionResult> LoadSettings(string settingsPage, string errorMessage = "")
        {

            var profile = await GetUserInformation();

            if (profile == null) {
                return RedirectToAction("Dashboard");
            }

            switch (settingsPage)
            {
                case "GeneralSettings":
                    return PartialView("Dashboard/Settings/_GeneralSettings");
                case "AccountSettings":
                    if (!string.IsNullOrEmpty(errorMessage)) {
                        ViewBag.ErrorMessage = errorMessage;
                    }
                    
                    return PartialView("Dashboard/Settings/_AccountSettings", profile);
                case "PrivacySettings":
                    return PartialView("Dashboard/Settings/_PrivacySettings", profile);
                case "ProfileSettings":
                    return PartialView("Dashboard/Settings/_ProfileSettings", profile);
                default:
                    return Content("Invalid settings page.");
            }
        }
        
        [HttpPost]
        public IActionResult VerifyUser([FromForm] string idToken)
        {
            // Validate the token received from Firebase on the server side
            if (string.IsNullOrEmpty(idToken))
            {
                return BadRequest("Invalid ID token");
            }

            // Here, you would validate the token using Firebase Admin SDK (or some other JWT verification method)
            // For simplicity, assuming it's a valid token, return success
            return Ok(new { message = "User verified successfully." });
        }


        #endregion

        #region ACCESS DENIED

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        #endregion

        #region ERROR HANDLING

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #endregion

        #region Language Switching

        [HttpPost]
        public IActionResult ChangeLanguage(string culture)
        {
            // Remove the existing cookie
            Response.Cookies.Delete("Culture");

            // Set the new culture cookie
            Response.Cookies.Append(
            "Culture",
            culture, 
            new CookieOptions { 
                Expires = DateTimeOffset.UtcNow.AddYears(1), 
                IsEssential = true, 
                SameSite = SameSiteMode.None, 
                Secure = true // Ensure this is true if running under HTTPS
            });

            // Redirect back to the previous page
            return Redirect(Request.Headers["Referer"].ToString());
        }



        #endregion

    }


    public IActionResult AdminDashboard() {
        // Check if the user is logged in and has admin privileges
    var username = HttpContext.Session.GetString("Username");
    if (username != null)
    {
        // Assuming you have an admin role in your DB_accounts table
        var user = _context.accounts.FirstOrDefault(u => u.username == username);
        if (user != null && user.IsAdmin) // Assuming you have an IsAdmin flag in the database
        {
            ViewBag.Username = username;
            return View();
        }
        else
        {
            // Redirect non-admin users to their regular dashboard
            return RedirectToAction("Dashboard");
        }
    }
    else
    {
        // Redirect to login if the session does not contain the username
        return RedirectToAction("Login");
    }

        //admin dashboard
        public IActionResult AdminDashboard()
        {
        // Check if the user is logged in and has admin privileges
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            // Assuming you have an admin role in your DB_accounts table
            var user = _context.accounts.FirstOrDefault(u => u.username == username);
            if (user != null && user.IsAdmin) // Assuming you have an IsAdmin flag in the database
            {
                ViewBag.Username = username;
                return View();
            }
            else
            {
                // Redirect non-admin users to their regular dashboard
                return RedirectToAction("Dashboard");
            }
        }
        else
        {
            // Redirect to login if the session does not contain the username
            return RedirectToAction("Login");
        }
    }

    }
}
