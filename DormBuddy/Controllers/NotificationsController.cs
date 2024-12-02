using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FirebaseAdmin.Messaging;

namespace DormBuddy.Controllers
{
    public class NotificationsController : BaseController
    {

        
        private readonly ILogger<NotificationsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly TimeZoneService _timeZoneService;

        private readonly IConfiguration _configuration;

        private readonly DBContext _context;
        private readonly IMemoryCache _memoryCache;

        public NotificationsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DBContext context,
            ILogger<NotificationsController> logger,
            IMemoryCache memoryCache,
            TimeZoneService timeZoneService,
            IConfiguration configuration) : base(userManager, signInManager, context, logger, memoryCache, timeZoneService, configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
            _timeZoneService = timeZoneService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index() {
            if (User?.Identity?.IsAuthenticated == true) {
                var user = await GetCurrentUserAsync();

                if (user == null) {
                    return RedirectToAction("AccountForms");
                }

                List<NotificationViewModel> notifs = await GetNotifications(user.Id);
                return View(notifs);
            }
            return RedirectToAction("AccountForms");
        }

        enum NOTIF_TYPE : int
        { 
            NOTIF_TYPE_DEFAULT = 1, // regular push notification
            NOTIF_TYPE_FRIENDREQUEST = 2 // friend request notification
        };

        // type: 1 = standard message, 2 = accept/decline, ....
        public async Task<IActionResult> CreateNotification(string userId, int type, string message, FriendsModel? request = null)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(message))
            {
                return BadRequest("Invalid user ID or message.");
            }

            try
            {
                var notification = new Notifications();

                switch (type)
                {
                    case (int)NOTIF_TYPE.NOTIF_TYPE_DEFAULT:

                    notification = new Notifications
                {
                    UserId = userId,
                    MessageType = type,
                    CreatedAt = DateTime.UtcNow,
                    Message = message
                };
                    break;
                    case (int)NOTIF_TYPE.NOTIF_TYPE_FRIENDREQUEST:

                    notification = new Notifications
                {
                    UserId = userId,
                    MessageType = type,
                    CreatedAt = DateTime.UtcNow,
                    Message = request?.Id.ToString()
                };
                    break;
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, NotificationId = notification.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification.");
                return StatusCode(500, "Internal server error.");
            }
        }


        public async Task<List<NotificationViewModel>> GetNotifications(string userId)
        {
            if (string.IsNullOrEmpty(userId)) 
            {
                return new List<NotificationViewModel>();
            }

            try
            {
                var notificationViewModels = await (from n in _context.Notifications
                                             join u in _context.Users on n.UserId equals u.Id
                                             join targetUser in _context.Users on n.Message equals targetUser.Id into targetUserGroup
                                             from targetUser in targetUserGroup.DefaultIfEmpty() // Allow for notifications without a targetUser
                                             where n.UserId == userId
                                             select new NotificationViewModel
                                             {
                                                 Id = n.Id,
                                                 UserId = n.UserId,
                                                 targetUserId = (n.MessageType == 2 && targetUser != null) ? targetUser.Id : null,
                                                 targetUserName = (n.MessageType == 2 && targetUser != null) ? targetUser.UserName : null,
                                                 Message = n.Message ?? "",
                                                 MessageType = n.MessageType,
                                                 CreatedAt = n.CreatedAt
                                             }).ToListAsync();

                foreach (var n in notificationViewModels) {
                    if (n.MessageType == 2) {
                        n.Message = n.targetUserName + " would like to be your friend!";
                    }
                }

                if (notificationViewModels == null || notificationViewModels.Count == 0)
                {
                    return new List<NotificationViewModel>();
                }


                await _context.SaveChangesAsync();

                return notificationViewModels;
            }
            catch (Exception) { return new List<NotificationViewModel>(); }
        }
        public async Task<IActionResult> RemoveNotification(Notifications not)
        {
            if (string.IsNullOrEmpty(not.UserId)) 
            {
                return BadRequest("User could not be found!");
            }

            if (not == null)
            {
                return BadRequest("Notification was not found!");
            }

            _context.Remove(not);
            await _context.SaveChangesAsync();
            return Ok("Notification removed");
        }

        public async Task<IActionResult> RemoveNotificationById(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return BadRequest("Notification was not found!");
            }

            _context.Remove(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Notifications");
        }

    }
}