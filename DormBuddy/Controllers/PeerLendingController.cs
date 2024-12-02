using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var loans = await _dbContext.PeerLendings
                .Where(l => l.UserId == user.Id || l.BorrowerId == user.Id)
                .ToListAsync();

            var groupId = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Select(gm => gm.GroupId)
                .FirstOrDefaultAsync();

            if (groupId != 0)
            {
                var groupMembers = await _dbContext.GroupMembers
                    .Where(gm => gm.GroupId == groupId)
                    .ToListAsync();

                var userIds = groupMembers.Select(gm => gm.UserId).ToList();
                var users = await _dbContext.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                ViewBag.GroupMembers = groupMembers;
                ViewBag.Users = users;

                Console.WriteLine($"Group found. GroupId: {groupId}, Members: {groupMembers.Count}, Users: {users.Count}");
            }
            else
            {
                ViewBag.GroupMembers = new List<GroupMemberModel>();
                ViewBag.Users = new List<ApplicationUser>();
                TempData["error"] = "You are not part of any group. Please join or create a group to use this feature.";

                Console.WriteLine($"No group found for user: {user.Id}");
            }

            var newLoan = new PeerLendingModel { UserId = user.Id };

            // Calculate stats
            ViewData["TotalLent"] = loans.Any() ? loans.Sum(l => l.Amount) : 0m;
            ViewData["ActiveLoans"] = loans.Count(l => !l.IsRepaid);
            ViewData["OverdueLoans"] = loans.Count(l => !l.IsRepaid && l.DueDate < DateTime.Now);
            ViewData["DueSoonLoans"] = loans.Count(l => !l.IsRepaid && 
                l.DueDate > DateTime.Now && 
                l.DueDate <= DateTime.Now.AddDays(3));

            // Return view with debug information if needed
            return View("~/Views/Account/Dashboard/Lending.cshtml", Tuple.Create(loans, newLoan));
        }

        // POST: /PeerLending/AddLoan
        [HttpPost]
        public async Task<IActionResult> AddLoan(PeerLendingModel model, string DueTimeHour, string DueTimeMinute, string DueTimeAMPM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            model.UserId = user.Id;
            ModelState.Remove(nameof(PeerLendingModel.UserId));

            if (ModelState.IsValid)
            {
                var dueTime = $"{model.DueDate:yyyy-MM-dd} {DueTimeHour}:{DueTimeMinute} {DueTimeAMPM}";
                if (DateTime.TryParse(dueTime, out DateTime parsedDate))
                {
                    model.DueDate = parsedDate;

                    _dbContext.PeerLendings.Add(model);
                    await _dbContext.SaveChangesAsync();

                    var lenderName = $"{user.FirstName} {user.LastName}";
                    var borrower = await _dbContext.Users.FindAsync(model.BorrowerId);
                    var borrowerName = borrower != null ? $"{borrower.FirstName} {borrower.LastName}" : "Unknown";

                    TempData["message"] = $"Loan from <b>{lenderName}</b> for <b>{borrowerName}</b> created successfully!";
                }
                else
                {
                    TempData["error"] = "Invalid date or time format.";
                    ModelState.AddModelError("DueDate", "Invalid date or time format.");
                }
            }
            else
            {
                TempData["error"] = "Invalid loan data.";
            }

            var loans = await _dbContext.PeerLendings
                .Where(l => l.UserId == user.Id || l.BorrowerId == user.Id)
                .ToListAsync();

            var groupId = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Select(gm => gm.GroupId)
                .FirstOrDefaultAsync();

            if (groupId != 0)
            {
                var groupMembers = await _dbContext.GroupMembers
                    .Where(gm => gm.GroupId == groupId)
                    .ToListAsync();

                var userIds = groupMembers.Select(gm => gm.UserId).ToList();
                var users = await _dbContext.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                ViewBag.GroupMembers = groupMembers;
                ViewBag.Users = users;

                Console.WriteLine($"Group found. GroupId: {groupId}, Members: {groupMembers.Count}, Users: {users.Count}");
            }
            else
            {
                ViewBag.GroupMembers = new List<GroupMemberModel>();
                ViewBag.Users = new List<ApplicationUser>();
                TempData["error"] = "You are not part of any group. Please join or create a group to use this feature.";

                Console.WriteLine($"No group found for user: {user.Id}");
            }

            var newLoan = new PeerLendingModel { UserId = user.Id };

            // Calculate stats
            ViewData["TotalLent"] = loans.Any() ? loans.Sum(l => l.Amount) : 0m;
            ViewData["ActiveLoans"] = loans.Count(l => !l.IsRepaid);
            ViewData["OverdueLoans"] = loans.Count(l => !l.IsRepaid && l.DueDate < DateTime.Now);
            ViewData["DueSoonLoans"] = loans.Count(l => !l.IsRepaid && 
                l.DueDate > DateTime.Now && 
                l.DueDate <= DateTime.Now.AddDays(3));

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
                var lenderName = $"{user.FirstName} {user.LastName}";
                var borrower = await _dbContext.Users.FindAsync(loan.BorrowerId);
                var borrowerName = borrower != null ? $"{borrower.FirstName} {borrower.LastName}" : "Unknown";

                _dbContext.PeerLendings.Remove(loan);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = $"Loan from <b>{lenderName}</b> for <b>{borrowerName}</b> deleted successfully!";
            }
            else
            {
                TempData["error"] = "Error: Loan not found.";
            }

            var loans = await _dbContext.PeerLendings
                .Where(l => l.UserId == user.Id || l.BorrowerId == user.Id)
                .ToListAsync();

            var groupId = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Select(gm => gm.GroupId)
                .FirstOrDefaultAsync();

            if (groupId != 0)
            {
                var groupMembers = await _dbContext.GroupMembers
                    .Where(gm => gm.GroupId == groupId)
                    .ToListAsync();

                var userIds = groupMembers.Select(gm => gm.UserId).ToList();
                var users = await _dbContext.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                ViewBag.GroupMembers = groupMembers;
                ViewBag.Users = users;

                Console.WriteLine($"Group found. GroupId: {groupId}, Members: {groupMembers.Count}, Users: {users.Count}");
            }
            else
            {
                ViewBag.GroupMembers = new List<GroupMemberModel>();
                ViewBag.Users = new List<ApplicationUser>();
                TempData["error"] = "You are not part of any group. Please join or create a group to use this feature.";

                Console.WriteLine($"No group found for user: {user.Id}");
            }

            var newLoan = new PeerLendingModel { UserId = user.Id };

            // Calculate stats
            ViewData["TotalLent"] = loans.Any() ? loans.Sum(l => l.Amount) : 0m;
            ViewData["ActiveLoans"] = loans.Count(l => !l.IsRepaid);
            ViewData["OverdueLoans"] = loans.Count(l => !l.IsRepaid && l.DueDate < DateTime.Now);
            ViewData["DueSoonLoans"] = loans.Count(l => !l.IsRepaid && 
                l.DueDate > DateTime.Now && 
                l.DueDate <= DateTime.Now.AddDays(3));

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