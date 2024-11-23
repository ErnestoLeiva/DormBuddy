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

namespace DormBuddy.Controllers
{
    public class BaseController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly DBContext _context;
        
        private readonly ILogger<BaseController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeZoneService _timezoneService;

        private readonly int REVALIDATE_TIME = 30; // in minutes

        public BaseController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DBContext context,
            ILogger<BaseController> logger,
            IMemoryCache memoryCache,
            TimeZoneService timezoneService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
            _timezoneService = timezoneService;
        }

        protected async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        protected async Task<UserProfile> GetUserInformation(string uname = null)
        {
            var user = await GetCurrentUserAsync();

            if (!string.IsNullOrEmpty(uname))
            {
                user = await _userManager.FindByNameAsync(uname);
            }

            if (user == null)
            {
                return null;
            }

            var cacheKey = $"userProfile_{user.Id}";

            // Check the cache first
            if (_memoryCache.TryGetValue(cacheKey, out UserProfile cachedProfile))
            {
                // Attach the cached profile if it is detached
                EnsureProfileAttached(cachedProfile);
                return cachedProfile;
            }

            // Fetch the profile from the database (tracked entity)
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            // If no profile exists, create a new one
            if (profile == null)
            {
                try
                {
                    profile = BuildUserProfile(user, null);
                    await _context.UserProfiles.AddAsync(profile);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating profile for user {UserId}", user.Id);
                    throw new Exception("Failed to create a user profile. Please contact support.");
                }
            }
            else
            {
                // If the profile exists, ensure it's attached
                EnsureProfileAttached(profile);

                // Update the profile with new data
                profile = BuildUserProfile(user, profile);
                await _context.SaveChangesAsync();
            }

            // Update the cache with the tracked entity
            _memoryCache.Set(cacheKey, profile, TimeSpan.FromMinutes(REVALIDATE_TIME));

            // Return the profile
            return profile;
        }

        // Helper method to ensure the profile is attached
        protected void EnsureProfileAttached(UserProfile profile)
        {

            var entries = _context.ChangeTracker.Entries();
            foreach (var entry in entries) {
                entry.State = EntityState.Detached;
            }

            var ent = _context.Entry(profile);
            _context.UserProfiles.Attach(profile);
        }

        protected void RevalidateCache(UserProfile profile) {
            int time = REVALIDATE_TIME; // in minutes
            var cacheKey = $"userProfile_{profile.UserId}";
                _memoryCache.Remove(cacheKey);
                _memoryCache.Set(cacheKey, profile, TimeSpan.FromMinutes(time)); // Optional: refresh cache
        }

        protected UserProfile BuildUserProfile(ApplicationUser user, UserProfile profile) {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            
            return new UserProfile
            {
                Id = profile?.Id ?? 0,

                UserId = user.Id,
                User = user,

                Bio = profile?.Bio,
                ProfileImageUrl = profile?.ProfileImageUrl ?? string.Empty,
                DateOfBirth = profile != null ? profile.DateOfBirth : DateTime.MinValue,

                FacebookUrl = profile?.FacebookUrl ?? string.Empty,
                TwitterUrl = profile?.TwitterUrl ?? string.Empty,
                LinkedInUrl = profile?.LinkedInUrl ?? string.Empty,
                InstagramUrl = profile?.InstagramUrl ?? string.Empty,

                Preferred_Language = profile?.Preferred_Language ?? "en",

                ReceiveEmailNotifications = profile != null ? profile.ReceiveEmailNotifications : true,
                ProfileVisibleToPublic = profile != null ? profile.ProfileVisibleToPublic : true,

                JobTitle = profile?.JobTitle ?? string.Empty,
                CompanyName = profile?.CompanyName ?? string.Empty,
                SchoolName = profile?.SchoolName ?? string.Empty,

                LastLogin = profile != null ? profile.LastLogin : DateTime.UtcNow,

                Verified = profile != null ? profile.Verified : false

            };
        }

        public async Task<IActionResult> getDBoardMessages(int ChatType)
        {
            var messages = await _context.DashboardChatModel
                .Where(m => m.type == ChatType)
                .OrderByDescending(m => m.sent_at)
                .Take(20)
                .ToListAsync();

            var userIds = messages.Select(m => m.UserId).Distinct().ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var response = messages
                .OrderBy(m => m.sent_at)
                .Select(m =>
                {
                    var user = users.FirstOrDefault(u => u.Id == m.UserId);
                    if (user == null)
                    {
                        throw new Exception($"User not found for UserId: {m.UserId}");
                    }

                    return new
                    {
                        Id = m.Id,
                        User = user,  
                        type = m.type,
                        sent_at = getCurrentTimeFromUTC(m.sent_at).ToString("yyyy-MM-dd HH:mm:ss tt"),
                        message = m.message
                    };
                });

            return Ok(new { r = response });
        }


        public async Task sendDBoardMessage(int ChatType, string text) {

            var user = await GetCurrentUserAsync();

            var message = new DashboardChatModel{
                UserId = user.Id,
                type =  ChatType,
                sent_at = DateTime.UtcNow,
                message = text
            };

            await _context.DashboardChatModel.AddAsync(message);

            await _context.SaveChangesAsync();

        }

                [HttpPost]
        public async Task<IActionResult> ChangeTimeZone(string timeZone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                Response.Cookies.Append(
                "UserTimeZone",
                timeZone, 
                new CookieOptions { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1), 
                    IsEssential = true, 
                    SameSite = SameSiteMode.None, 
                    Secure = true 
                });
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult GetCurrentTime()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Get the user's time zone from the cookie
            string userTimeZoneId = Request.Cookies["UserTimeZone"] ?? "UTC";

            // Convert the UTC time to the user's local time
            DateTime eventLocalTime = _timezoneService.ConvertToLocal(utcNow, userTimeZoneId);

            // Return the local time as a string
            return Content(eventLocalTime.ToString("F"));
        }

        public DateTime getCurrentTimeFromUTC(DateTime date) {
            DateTime time = date;

            string userTimeZoneId = Request.Cookies["UserTimeZone"] ?? "UTC";

            DateTime local = _timezoneService.ConvertToLocal(time, userTimeZoneId);

            return local;
        }

    }
}