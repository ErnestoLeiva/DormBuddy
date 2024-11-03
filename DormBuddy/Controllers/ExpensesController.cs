using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace DormBuddy.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ILogger<TasksController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _dbContext;
    
        public ExpensesController(ILogger<TasksController> logger, DBContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var expenses = await _dbContext.Expenses
                    .Where(e => e.UserId == user.Id)
                    .ToListAsync();

                var newExpense = new ExpenseModel { UserId = user.Id };
                return View("~/Views/Account/Dashboard/Expenses.cshtml", Tuple.Create(expenses, newExpense));
            }

            return RedirectToAction("Login", "Account");
        }




        // POST: /Expenses/AddExpense
        [HttpPost]
        public async Task<IActionResult> AddExpense(ExpenseModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            model.UserId = user.Id;
            model.User = user;

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _dbContext.Expenses.Add(model);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = "Expense added successfully!";
            }
            else
            {
                TempData["error"] = "Error: Invalid expense data.";
            }

            var expenses = await _dbContext.Expenses.Where(e => e.UserId == user.Id).ToListAsync();
            var newExpense = new ExpenseModel { UserId = user.Id };
            return View("~/Views/Account/Dashboard/Expenses.cshtml", Tuple.Create(expenses, newExpense));
        }


        // POST: /Expenses/DeleteExpense
        [HttpPost]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var expense = await _dbContext.Expenses.FindAsync(expenseId);
            if (expense != null)
            {
                _dbContext.Expenses.Remove(expense);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = $"Expense \"{expense.ExpenseName}\" deleted successfully!";
            }
            else
            {
                TempData["error"] = "Error: Expense not found.";
            }

            return RedirectToAction("Index");
        }
    }
}