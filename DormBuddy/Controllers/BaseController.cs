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

        private readonly int REVALIDATE_TIME = 30; // in minutes

        public BaseController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DBContext context,
            ILogger<BaseController> logger,
            IMemoryCache memoryCache
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
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

        private UserProfile BuildUserProfile(ApplicationUser user, UserProfile profile) {
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

                AccountStatus = profile?.AccountStatus ?? "Not Verified"

            };
        }

        

        }
}