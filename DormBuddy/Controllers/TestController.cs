using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DormBuddy.Controllers;

public class TestController : Controller
{

    private readonly DBContext _context;

    public TestController(DBContext context) {
        _context = context;
    }

    //public Task<IActionResult> Index()
    public IActionResult Index()
    {
        /*
        try
        {
            var data = await _context.accounts.ToListAsync();
            return View(data);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
        */
        return View();
    }

}