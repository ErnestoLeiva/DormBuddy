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

        #region TASKS

        // GET: /Tasks/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var tasks = await _dbContext.Tasks
                    .Where(t => t.UserId == user.Id)
                    .ToListAsync();

                var newTask = new TaskModel { UserId = user.Id }; // For the form on tasks.cshftml
                return View("~/Views/Account/Dashboard/Tasks.cshtml", Tuple.Create(tasks, newTask));
            }

            return RedirectToAction("Login", "Account");
        }

        // POST: /Tasks/Add
        [HttpPost]
        public async Task<IActionResult> AddTask(TaskModel model, string DueTimeHour, string DueTimeMinute, string DueTimeAMPM)
        {
            if (ModelState.IsValid)
            {

                var dueTime = $"{model.DueDate:yyyy-MM-dd} {DueTimeHour}:{DueTimeMinute} {DueTimeAMPM}";
                if (DateTime.TryParse(dueTime, out DateTime parsedDate))
                {
                    model.DueDate = parsedDate;
                    model.IsCompleted = false;

                    _dbContext.Tasks.Add(model);
                    await _dbContext.SaveChangesAsync();

                    TempData["message"] = "Task added successfully!";
                }
                else
                {
                    ViewBag.ErrorMessage = "Error: Invalid date format.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Error: Invalid task input.";
            }

            var tasks = await _dbContext.Tasks.Where(t => t.UserId == model.UserId).ToListAsync();
            var newTask = new TaskModel();
            return View("~/Views/Account/Dashboard/Tasks.cshtml", Tuple.Create(tasks, newTask));
        }

        // POST: /Tasks/Delete
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            if (taskId <= 0)
            {
                ViewBag.ErrorMessage = "Invalid Task ID.";
                return RedirectToAction("Index");
            }

            try
            {
                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task != null)
                {
                    var taskname = task.TaskName;
                    _dbContext.Tasks.Remove(task);
                    await _dbContext.SaveChangesAsync();
                    TempData["message"] = $"Task \"{taskname}\" deleted successfully!";
                }
                else
                {
                    ViewBag.ErrorMessage = "Error: Task not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting task with ID {taskId}: {ex.Message}");
                ViewBag.ErrorMessage = "Error: Could not delete the task.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Tasks/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int taskId)
        {
            Console.WriteLine("\n\n\nREACHED ACTION\n\n\n");
            
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
                Console.WriteLine($"Exception occurred: {ex.Message}\n{ex.StackTrace}");
                _logger.LogError($"Error toggling task status with ID {taskId}: {ex.Message}");
                return Json(new { success = false, message = "Error: Could not update the task." });
            }
        }

        #endregion
    }
}