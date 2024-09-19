using System.Diagnostics;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using Microsoft.EntityFrameworkCore;


namespace DormBuddy.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly DBContext _context;

    public AccountController(ILogger<AccountController> logger, DBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult AccountForms()
    {
    return View();
    }

    public IActionResult Login()
    {

        // Check if the user is logged in by checking the session
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            return RedirectToAction("Dashboard", "Account");
        }

        return View();
    }

    #region LOGIN HANDLING
    // POST: /Account/Login -- (This handles a user clicking the "login" button on the login page)
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var acc = await _context.accounts.FirstOrDefaultAsync(u => u.username == username || u.email == username);

        if (acc != null) {
            
            var hashCheck = BCrypt.Net.BCrypt.Verify(password, acc.password);

            if (hashCheck) {
                HttpContext.Session.SetString("Username", acc.username);
                return RedirectToAction("Dashboard", "Account");
            }

        }

        ViewBag.ErrorMessage = "Invalid credentials, try again!";
        return View("AccountForms");
    }

    public IActionResult Signup() {

        // Check if the user is logged in by checking the session
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            return RedirectToAction("Dashboard", "Account");
        }

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
                    // Assign the default role to the user
                    await _userManager.AddToRoleAsync(user, "User");

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
                    //await _userManager.AddToRoleAsync(user, "Moderator");
                    //await _userManager.RemoveFromRoleAsync(user, "Moderator");
                    var roles = await _userManager.GetRolesAsync(user);
                    ViewBag.Username = $"{user.FirstName} {user.LastName}";
                    ViewBag.UserRoles = string.Join(", ", roles);
                }
                return View();
            }

            if (password != reenterpassword) {
                ViewBag.ErrorMessage = "Passwords do not match!";
                return View("AccountForms");
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            var newAccount = new DB_accounts {

                email = email,
                username = username,
                password = hash,
                firstname = firstname,
                lastname = lastname,
                balance = 0

            };

            if (accFromUsername == null && accFromEmail == null) 
            {
                _context.accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("Dashboard", "Account");
            }
        }

        return View("AccountForms");

    }

    // GET: /Account/Dashboard
    public IActionResult Dashboard()
    {
        // Check if the user is logged in by checking the session
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            ViewBag.Username = username;
            return View();
        }
        else
        {
            // Redirect to login if the session does not contain the username
            return RedirectToAction("Login");
        }

        #endregion

        #region ACCESS DENIED HANDLING
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult SignOutAccount() {

        // Check if the user is logged in by checking the session
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
        {
            return RedirectToAction("Dashboard", "Account");
        }

        HttpContext.Session.Remove("Username");
        return RedirectToAction("HomeLogin", "Home");
    }
}
