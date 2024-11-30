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
    public class PeerLendingController : Controller
    {
        private readonly ILogger<PeerLendingController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _dbContext;

        public PeerLendingController(
            ILogger<PeerLendingController> logger,
            UserManager<ApplicationUser> userManager,
            DBContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: /PeerLending/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var loans = await _dbContext.PeerLendings
                    .Where(l => l.UserId == user.Id)
                    .ToListAsync();

                var newLoan = new PeerLendingModel { UserId = user.Id };

                ViewData["TotalLent"] = loans.Any() ? loans.Sum(l => l.Amount) : 0m;
                ViewData["ActiveLoans"] = loans.Count(l => !l.IsRepaid);
                ViewData["OverdueLoans"] = loans.Count(l => !l.IsRepaid && l.DueDate < DateTime.Now);
                ViewData["DueSoonLoans"] = loans.Count(l => !l.IsRepaid && 
                    l.DueDate > DateTime.Now && 
                    l.DueDate <= DateTime.Now.AddDays(3));

                return View("~/Views/Account/Dashboard/Lending.cshtml", Tuple.Create(loans, newLoan));
            }

            return RedirectToAction("Login", "Account");
        }

        // POST: /PeerLending/AddLoan
        [HttpPost]
        public async Task<IActionResult> AddLoan(PeerLendingModel model, string DueTimeHour, string DueTimeMinute, string DueTimeAMPM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var dueTime = $"{model.DueDate:yyyy-MM-dd} {DueTimeHour}:{DueTimeMinute} {DueTimeAMPM}";
                if (DateTime.TryParse(dueTime, out DateTime parsedDate))
                {
                    model.DueDate = parsedDate;
                    model.UserId = user.Id;

                    _dbContext.PeerLendings.Add(model);
                    await _dbContext.SaveChangesAsync();
                    
                    TempData["message"] = $"Loan for borrower \"{model.BorrowerId}\" added successfully!";
                }
                else
                {
                    TempData["error"] = "Invalid date or time format.";
                    ModelState.AddModelError("DueDate", "Invalid date or time format.");
                    return View("Index", Tuple.Create(await _dbContext.PeerLendings.Where(l => l.UserId == user.Id).ToListAsync(), new PeerLendingModel { UserId = user.Id }));
                }
            }
            else
            {
                TempData["error"] = "Invalid loan data.";
                return View("Index", Tuple.Create(await _dbContext.PeerLendings.Where(l => l.UserId == user.Id).ToListAsync(), new PeerLendingModel { UserId = user.Id }));
            }

            var loans = await _dbContext.PeerLendings.Where(l => l.UserId == user.Id).ToListAsync();
            var newLoan = new PeerLendingModel { UserId = user.Id };
            return View("~/Views/Account/Dashboard/Lending.cshtml", Tuple.Create(loans, newLoan));
        }

        // POST: /PeerLending/DeleteLoan
        [HttpPost]
        public async Task<IActionResult> DeleteLoan(int loanId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var loan = await _dbContext.PeerLendings.FindAsync(loanId);
            if (loan != null)
            {
                var borrowerName = loan.BorrowerId;
                _dbContext.PeerLendings.Remove(loan);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = $"Loan for borrower \"{borrowerName}\" deleted successfully!";
            }
            else
            {
                TempData["error"] = "Error: Loan not found.";
            }

            var loans = await _dbContext.PeerLendings.Where(l => l.UserId == user.Id).ToListAsync();
            var newLoan = new PeerLendingModel { UserId = user.Id };
            return View("~/Views/Account/Dashboard/Lending.cshtml", Tuple.Create(loans, newLoan));
        }

        // POST: /PeerLending/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int loanId)
        {
            var loan = await _dbContext.PeerLendings.FindAsync(loanId);
            if (loan != null)
            {
                loan.IsRepaid = !loan.IsRepaid;
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Loan status updated!" });
            }
            return Json(new { success = false, message = "Error: Loan not found." });
        }
    }
}