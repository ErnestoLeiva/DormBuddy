using System.Diagnostics;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly RoleManager<IdentityRole> _roleManager;

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
            _emailSender = emailSender;
            _timeZoneService = timeZoneService;
            _configuration = configuration;
            _context = context;
            _memoryCache = memoryCache;
            _roleManager = roleManager;     
        }

        #region ACCOUNT FORMS

        public IActionResult AccountForms()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View();
        }

        #endregion
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

        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View("AccountForms");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Invalid credentials, try again!";
                return View("AccountForms");
            }

            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid Username/Email entered: User does not exist.";
                return View("AccountForms");
            }

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

                ViewBag.ErrorMessage = "Your email has not been confirmed. Please check your email for confirmation.";
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
                    return View("AccountForms""AccountForms");
                }

                await _userManager.AccessFailedAsync(user);

                var failed = await _userManager.GetAccessFailedCountAsync(user);
                var max = _userManager.Options.Lockout.MaxFailedAccessAttempts;

                var remaining = max - failed;

                ViewBag.ErrorMessage = "Invalid credentials, try again!\nRemaining attempts: " + remaining;
            }

            
            return View("AccountForms""AccountForms");
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

        public IActionResult Signup()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

            return View("AccountForms");
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
                return View("AccountForms""AccountForms");
            }

            if (await _userManager.FindByNameAsync(username) != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                ViewBag.ErrorMessage = "Username is already taken!";
                return View("AccountForms""AccountForms");
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                ViewBag.ErrorMessage = "Email is already registered!";
                return View("AccountForms""AccountForms");
            }

            var passwordValidator = new PasswordValidator<ApplicationUser>();
            var placeholder = new ApplicationUser();
            var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, placeholder, password);

            if (!passwordValidationResult.Succeeded)
            {
                foreach (var error in passwordValidationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.ErrorMessage = "Password does not meet the requirements!";
                return View("AccountForms""AccountForms");
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
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("Activation", "Account", new { userId = user.Id, token }, Request.Scheme);

                if (!string.IsNullOrEmpty(confirmationLink))
                    await _emailSender.SendActivationEmail(user, confirmationLink);
                else
                    Console.WriteLine("Confirmation link is null! Activation email not sent to user!");

                TempData["message"] = "Email verification has been sent!";
                return View("AccountForms");
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("AccountForms");
        }

        #endregion

        #region ACCOUNT CONFIRMATION

        [HttpGet]
        public async Task<IActionResult> Activation(string userId, string token)
        {
            if (userId == null || token == null)
            {
                ViewBag.ErrorMessage = "Invalid email confirmation!";
                return RedirectToAction("AccountForms");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"User not found with id {userId}");
                ViewBag.ErrorMessage = "User not found";
                return RedirectToAction("AccountForms");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["message"] = "Email confirmation is successful!";
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard");
            }

            ViewBag.ErrorMessage = "Email confirmation failure!";
            return RedirectToAction("AccountForms");
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
                if (result.Succeeded)
                {
                    // Assign "User" role to the new account
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard");
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

        #region LOGOUT

        public async Task<IActionResult> Logout()
        {
            if (User?.Identity?.IsAuthenticated == true)
                await _signInManager.SignOutAsync();

            return RedirectToAction("AccountForms");
        }

        #endregion

        #region DASHBOARD

        public async Task<IActionResult> Dashboard()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
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

            return RedirectToAction("AccountForms");
        }

        #endregion

        #region DASHBOARD SECTIONS

        public IActionResult Tasks() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Tasks.cshtml") : RedirectToAction("AccountForms");

        public IActionResult Expenses() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Expenses.cshtml") : RedirectToAction("AccountForms");

        public IActionResult Lending() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Lending.cshtml") : RedirectToAction("AccountForms");

        public IActionResult Notifications() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Notifications.cshtml") : RedirectToAction("AccountForms");


        public IActionResult Settings() {
            if (User?.Identity?.IsAuthenticated == true) {

                string loadPage = Request.Query["page"].ToString() ?? "";

                var allowedPages = new List<string> { "AccountSettings", "GeneralSettings", "PrivacySettings", "ProfileSettings" };

                // Check if the 'page' is a valid value from the list
                if (!allowedPages.Contains(loadPage) && !string.IsNullOrEmpty(loadPage))
                {
                    return BadRequest("Invalid page parameter.");
                }

                ViewBag.LoadPage = loadPage;
                return View("~/Views/Account/Dashboard/Settings.cshtml");
            } else {
                return RedirectToAction("AccountForms");
            }
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

        public async Task<IActionResult> LoadSettings(string settingsPage)
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
}
