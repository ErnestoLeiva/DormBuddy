using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DormBuddy.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ILogger<TasksController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _dbContext;

        public TasksController(
            ILogger<TasksController> logger,
            UserManager<ApplicationUser> userManager,
            DBContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: /Tasks/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var (tasks, newTask) = await LoadTaskData(user);

            return View("~/Views/Account/Dashboard/Tasks.cshtml", Tuple.Create(tasks, newTask));
        }

        // POST: /Tasks/Add
        [HttpPost]
        public async Task<IActionResult> AddTask(TaskModel model, string[] AssignedUserIds, string DueTimeHour, string DueTimeMinute, string DueTimeAMPM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(nameof(TaskModel.UserId), "User not found.");
                return RedirectToAction("Login", "Account");
            }

            model.UserId = user.Id;
            ModelState.Remove(nameof(TaskModel.UserId));

            if (AssignedUserIds != null && AssignedUserIds.Length > 0)
            {
                model.AssignedTo = string.Join(",", AssignedUserIds);

                ModelState.Remove(nameof(TaskModel.AssignedTo));
            }
            else
            {
                ModelState.AddModelError(nameof(TaskModel.AssignedTo), "At least one user must be assigned.");
            }

            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var dueTime = $"{model.DueDate:yyyy-MM-dd} {DueTimeHour}:{DueTimeMinute} {DueTimeAMPM}";
                    if (DateTime.TryParse(dueTime, out DateTime parsedDate))
                    {
                        model.DueDate = parsedDate;
                        model.IsCompleted = false;

                        _dbContext.Tasks.Add(model);
                        await _dbContext.SaveChangesAsync();

                        TempData["message"] = $"Task <b>{model.TaskName}</b> added successfully!";
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(TaskModel.DueDate), "Invalid date format.");
                    }
                } 
                else 
                {
                    ModelState.AddModelError(nameof(TaskModel.DueDate), "Invalid date format.");
                }
            }
            else
            {
                Console.WriteLine("INVALID MODEL STATE");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            var (tasks, newTask) = await LoadTaskData(user);

            return View("~/Views/Account/Dashboard/Tasks.cshtml", Tuple.Create(tasks, newTask));
        }

        // POST: /Tasks/Delete
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (taskId <= 0)
            {
                TempData["error"] = "Invalid Task ID.";
            }
            else
            {
                try
                {
                    var task = await _dbContext.Tasks.FirstOrDefaultAsync(t =>
                        t.Id == taskId &&
                        (t.UserId == user.Id || (t.AssignedTo != null && t.AssignedTo.Contains(user.Id))));

                    if (task != null)
                    {
                        var taskname = task.TaskName;
                        _dbContext.Tasks.Remove(task);
                        await _dbContext.SaveChangesAsync();
                        TempData["message"] = $"Task <b>{taskname}</b> deleted successfully!";
                    }
                    else
                    {
                        TempData["error"] = "Error: Task not found or you do not have permission to delete it.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error deleting task with ID {taskId}: {ex.Message}");
                    TempData["error"] = "Error: Could not delete the task.";
                }
            }

            var (tasks, newTask) = await LoadTaskData(user);
            
            return View("~/Views/Account/Dashboard/Tasks.cshtml", Tuple.Create(tasks, newTask));
        }

        // POST: /Tasks/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int taskId)
        {
            if (taskId <= 0)
            {
                return Json(new { success = false, message = "Invalid Task ID." });
            }

            try
            {
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task != null)
                {
                    task.IsCompleted = !task.IsCompleted;
                    await _dbContext.SaveChangesAsync();
                    return Json(new { success = true, message = $"Task \"{task.TaskName}\" status updated!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error: Task not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error toggling task status with ID {taskId}: {ex.Message}");
                return Json(new { success = false, message = "Error: Could not update the task." });
            }
        }
    
        // load data function
        private async Task<(List<TaskModel> tasks, TaskModel newTask)> LoadTaskData(ApplicationUser user)
        {
            var tasks = await _dbContext.Tasks
                .Where(t => (t.AssignedTo != null && t.AssignedTo.Contains(user.Id)) || t.UserId == user.Id)
                .ToListAsync();

            var groupId = await _dbContext.GroupMembers
                .Where(gm => gm.UserId == user.Id)
                .Select(gm => gm.GroupId)
                .FirstOrDefaultAsync();


            if (groupId == 0)
            {
                ViewBag.GroupMembers = new List<GroupMemberModel>();
                ViewBag.Users = new List<ApplicationUser>();
                TempData["error"] = "You are not part of any group. Please join or create a group to use this feature.";
            }
            else
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
            }

            var newTask = new TaskModel { UserId = user.Id };

            return(tasks, newTask);
        }
    }
}
