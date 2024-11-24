using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly DormBuddy.Models.IEmailSender _emailSender;

        private readonly TimeZoneService _timeZoneService;

        private readonly IConfiguration _configuration;

        private readonly DBContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DormBuddy.Models.IEmailSender emailSender,
            TimeZoneService timeZoneService,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            DBContext context) : base(userManager, signInManager, context, logger, memoryCache, timeZoneService, configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _timeZoneService = timeZoneService;
            _configuration = configuration;
            _context = context;
            _memoryCache = memoryCache;
            _roleManager = roleManager;     
        }

        #region ACCOUNT FORMS

        // GET: /Account/AccountForms
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

    public IActionResult AccountForms()
    {
    return View();
    }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
                
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                    if (result.Succeeded)
                    {
                        
                        return RedirectToAction("Dashboard");
                    }

                    if (!user.EmailConfirmed) {

                        if (TempData["ResendCode"] != null) {
                            // resend email
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var confirmationLink = Url.Action("Activation", "Account", new { userId = user.Id, token }, Request.Scheme);
                            if (!string.IsNullOrEmpty(confirmationLink)) {
                                await _emailSender.SendActivationEmail(user, confirmationLink);
                                TempData["message"] = "Email confirmation has been sent!";
                            } else {
                                Console.WriteLine("Confirmation link is null! Activation email not sent to user!");
                            }
                            TempData["ResendCode"] = null;
                        } else {
                            ViewBag.ErrorMessage = "Check your email for confirmation! Need it to be resent? Try Logging in again!";
                            TempData["ResendCode"] = "true";
                        }

                        return View("AccountForms");
                    }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (result.Succeeded) {
                await _userManager.ResetAccessFailedCountAsync(user);

                //var profile = await getProfile(user);
                var profile = await GetUserInformation(user.UserName);

                await _signInManager.SignInAsync(user, rememberMe);

                profile.LastLogin = DateTime.UtcNow;
                 _context.SaveChanges();

                return RedirectToAction("Dashboard");

            } else {
                if (result.IsLockedOut) {
                    // send message of time left and return
                    var lockoutTime = await _userManager.GetLockoutEndDateAsync(user);
                    var timeRemaining = lockoutTime.Value - DateTimeOffset.Now;
                    ViewBag.ErrorMessage = "Account is locked out!\nRemaining: " + timeRemaining.Minutes + " minutes, " + timeRemaining.Seconds + " seconds.";
                    return View("AccountForms");
                }

                
            }
            ViewBag.ErrorMessage = "Invalid credentials, try again!";
            return View("AccountForms");
        }

        #endregion

        public async Task<UserProfile> getProfile(ApplicationUser user) {
            /*
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id
                };

                _context.UserProfiles.Add(profile);
            }

            return profile;
            */
            return await GetUserInformation(user.UserName);
        }

        public async Task<UserLastUpdate> getUserLastUpdate(ApplicationUser u) {
            var instance = await _context.UserLastUpdate.FirstOrDefaultAsync(p => p.UserId == u.Id);

            if (instance == null && u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))  
            {
                instance = new UserLastUpdate
                {
                    UserId = u.Id,
                    LastUpdate = DateTime.UtcNow
                };

                _context.UserLastUpdate.Add(instance);
            }

            return instance;
        }

        #region SIGN UP

        // GET: /Account/Signup
        // GET: /Account/Signup
        public IActionResult Signup()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        // POST: /Account/Signup
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
            if (ModelState.IsValid)
            {
                // Check if passwords match
                if (password != reenterpassword)
                {
                    ModelState.AddModelError(string.Empty, "Passwords do not match.");
                    ViewBag.ErrorMessage = "Passwords do not match!";
                    return View("AccountForms");
                }

                // Check if a user with the same username already exists
                        // Check if username exists
                var existingUserByName = await _userManager.FindByNameAsync(username);
                        if (existingUserByName != null)
                        {
                            ModelState.AddModelError(string.Empty, "Username is already taken.");
                            ViewBag.ErrorMessage = "Username is already taken!";
                            return View("AccountForms");
                        }

                // Check if a user with the same email already exists
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
                // Validate password
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var placeholder = new ApplicationUser();
            var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, placeholder, password);
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
                    TempData["message"] = "Email confirmation has been sent!";
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
                ViewBag.message = "Invalid email confirmation!";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                Console.WriteLine("User not found with id {userId}");
                ViewBag.message = "User not found";
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
                ViewBag.message = "Email confirmation failure!";
                return RedirectToAction("Login");
            }
        }
        #endregion

        #region FORGOT PASSWORD

        [HttpGet]
        public IActionResult ForgotPassword() {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View("~/Views/Account/Password/ForgotPassword.cshtml");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            
            if (ModelState.IsValid) {
                
                var user = await _userManager.FindByEmailAsync(model.Email ?? "");

                if (user == null || !user.EmailConfirmed) {
                    return RedirectToAction("ForgotPassword");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var url = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, token }, Request.Scheme);

                var email = await _emailSender.SendPasswordResetEmail(user, url ?? "null");

                TempData["message"] = "Reset link has been sent! Check your email!";
                return View("AccountForms");

            }
            return View("~/Views/Account/Password/ForgotPassword.cshtml", model);

        }

        [HttpGet]
        public ActionResult ResetPassword(string userId, string token)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
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
                    ViewBag.UserRoles = string.Join(", ", await _userManager.GetRolesAsync(user));
                    
                    // Get all roles
                    var roles = await _roleManager.Roles.ToListAsync();
                    ViewBag.Roles = await _roleManager.Roles.ToListAsync(); // Pass roles to the view

                    var currentCulture = CultureInfo.CurrentCulture.Name;
                    var currentUICulture = CultureInfo.CurrentUICulture.Name;

                    ViewBag.CultureInfo = $"Current Culture: {currentCulture}, UI Culture: {currentUICulture}";
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
            return RedirectToAction("Login");
        }

        // GET: /Account/Dashboard/Notifications
        public IActionResult Notifications()
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
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return View("~/Views/Account/Dashboard/Settings.cshtml");
            }
            return RedirectToAction("Login");
        }

        #endregion

        #region ACCESS DENIED

        #region ACCESS DENIED

        #region ACCESS DENIED HANDLING
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
}
