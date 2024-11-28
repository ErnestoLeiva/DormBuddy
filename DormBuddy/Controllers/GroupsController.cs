using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormBuddy.Controllers
{
    
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _dbContext;

        public GroupsController(UserManager<ApplicationUser> userManager, DBContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: /Groups
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var groups = await _dbContext.Groups.ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync();

            var users = await _dbContext.Users.ToListAsync();
            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }

        // POST: /Groups/Create
        [HttpPost]
        public async Task<IActionResult> CreateGroup(GroupModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ModelState.Remove(nameof(GroupModel.InvitationCode));
            ModelState.Remove(nameof(GroupModel.CreatedByUserId));

            if (ModelState.IsValid)
            {
                model.CreatedByUserId = user.Id;
                model.InvitationCode = Guid.NewGuid().ToString("N").Substring(0, 8);
                
                _dbContext.Groups.Add(model);
                await _dbContext.SaveChangesAsync();

                var groupMember = new GroupMemberModel
                {
                    GroupId = model.Id,
                    UserId = user.Id,
                    IsAdmin = true
                };

                _dbContext.GroupMembers.Add(groupMember);
                await _dbContext.SaveChangesAsync();

                TempData["message"] = $"Group created successfully! Your invitation code is {model.InvitationCode}.";
            }
            else
            {
                TempData["error"] = "Invalid group data.";
            }

            // Re-fetch data for the view
            var groups = await _dbContext.Groups.Include(g => g.Members).ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync();
            var users = await _dbContext.Users.ToListAsync(); // Refresh users

            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }


        // POST: /Groups/Join
        [HttpPost]
        public async Task<IActionResult> JoinGroup(string invitationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var group = await _dbContext.Groups.FirstOrDefaultAsync(g => g.InvitationCode == invitationCode);
            if (group == null)
            {
                TempData["error"] = "Invalid invitation code.";
            }
            else
            {
                var memberCount = await _dbContext.GroupMembers.CountAsync(gm => gm.GroupId == group.Id);
                if (memberCount >= group.MaxMembers)
                {
                    TempData["error"] = "Group is full.";
                }
                else if (await _dbContext.GroupMembers.AnyAsync(gm => gm.UserId == user.Id))
                {
                    TempData["error"] = "You are already in a group.";
                }
                else
                {
                    var groupMember = new GroupMemberModel
                    {
                        GroupId = group.Id,
                        UserId = user.Id
                    };

                    _dbContext.GroupMembers.Add(groupMember);
                    await _dbContext.SaveChangesAsync();
                    TempData["message"] = "Joined group successfully!";
                }
            }

            // Re-fetch data for the view
            var groups = await _dbContext.Groups.ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync();
            var users = await _dbContext.Users.ToListAsync(); // Refresh users

            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }

        // POST: /Groups/Leave
        [HttpPost]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var membership = await _dbContext.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == user.Id);

            if (membership != null)
            {
                _dbContext.GroupMembers.Remove(membership);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = "Left group successfully!";
            }
            else
            {
                TempData["error"] = "You are not a member of this group.";
            }

            // Re-fetch data for the view
            var groups = await _dbContext.Groups.ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync();
            var users = await _dbContext.Users.ToListAsync(); // Refresh users

            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }

        // POST: /Groups/KickMember
        [HttpPost]
        public async Task<IActionResult> KickMember(int groupId, string userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var groupMember = await _dbContext.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (groupMember != null)
            {
                _dbContext.GroupMembers.Remove(groupMember);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = "Member removed successfully.";
            }
            else
            {
                TempData["error"] = "Member not found in the group.";
            }

            // Re-fetch data for the view
            var groups = await _dbContext.Groups.ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .ThenInclude(g => g.Members)
                .FirstOrDefaultAsync();
            var users = await _dbContext.Users.ToListAsync(); // Refresh users

            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }

        // POST: /Groups/DeleteGroup
        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var group = await _dbContext.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null && group.CreatedByUserId == user.Id)
            {
                _dbContext.GroupMembers.RemoveRange(group.Members);
                _dbContext.Groups.Remove(group);
                await _dbContext.SaveChangesAsync();
                TempData["message"] = "Group deleted successfully.";
            }
            else
            {
                TempData["error"] = "You are not authorized to delete this group.";
            }

            // Re-fetch data for the view
            var groups = await _dbContext.Groups.ToListAsync();
            var userGroup = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync();
            var users = await _dbContext.Users.ToListAsync(); // Refresh users

            ViewBag.Users = users;

            return View("~/Views/Account/Dashboard/Groups.cshtml", Tuple.Create(groups, userGroup));
        }
    }

}

