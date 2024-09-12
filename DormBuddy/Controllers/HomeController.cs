using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;

namespace DormBuddy.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            return RedirectToAction("Dashboard", "Account");
        }
        return View();
    }

    public IActionResult HomeLogin()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username != null)
        {
            return RedirectToAction("Dashboard", "Account");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
