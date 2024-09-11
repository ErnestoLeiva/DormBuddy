using System.Diagnostics;
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
        return View();
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

    [HttpPost]
    public async Task<IActionResult> Signup(string email, string username, string password, string reenterpassword, string firstname, string lastname) {

        if (ModelState.IsValid)
        {

            var accFromUsername = await _context.accounts.FirstOrDefaultAsync(u => u.username == username);

            if (accFromUsername != null) { // Account already exists as checked from username
                ViewBag.ErrorMessage = "An account with that username already exists!";
                return View();
            }

            var accFromEmail = await _context.accounts.FirstOrDefaultAsync(u => u.email == email);

            if (accFromEmail != null) { // Account already exists as checked from username
                ViewBag.ErrorMessage = "An account with that email already exists!";
                return View();
            }

            if (password != reenterpassword) {
                ViewBag.ErrorMessage = "Passwords do not match!";
                return View();
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

        return View();

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
