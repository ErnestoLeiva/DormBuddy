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
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        #region ACCOUNT FORMS

        public IActionResult AccountForms()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard");

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
        public async Task<IActionResult> Login(string username, string password)
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

            if (result.Succeeded)
                return RedirectToAction("Dashboard");

            ViewBag.ErrorMessage = "Invalid credentials, try again!";
            return View("AccountForms");
        }

        #endregion

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
            var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, null, password);

            if (!passwordValidationResult.Succeeded)
            {
                foreach (var error in passwordValidationResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

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
            
            Console.WriteLine("part 1");
            if (ModelState.IsValid) {
                Console.WriteLine("part 2");
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

        public IActionResult Settings() => User?.Identity?.IsAuthenticated == true ? View("~/Views/Account/Dashboard/Settings.cshtml") : RedirectToAction("AccountForms");

        #endregion

        #region ACCESS DENIED

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        #endregion

        #region ERROR HANDLING

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #endregion
    }
}
