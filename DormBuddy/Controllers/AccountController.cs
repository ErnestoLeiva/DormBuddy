using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;


namespace DormBuddy.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    public IActionResult Login()
    {
        return View();
    }

    #region LOGIN HANDLING
    // POST: /Account/Login -- (This handles a user clicking the "login" button on the login page)
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // temp hardcode checks before we implement any real authentication methods
        if (username == "admin" && password == "password")  // Dummy check
        {
            // Store the username in Session to access it in the dashboard
            HttpContext.Session.SetString("Username", username);

            // Successful login - redirect to dashboard page
            return RedirectToAction("Dashboard", "Account");
        }
        else
        {
            // Set the error message in TempData so it's available after redirect
            TempData["ErrorMessage"] = "Invalid login credentials";

            // Redirect back to the HomeLogin page in HomeController
            return RedirectToAction("HomeLogin", "Home");
        }
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
}
