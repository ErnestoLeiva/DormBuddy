using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace DormBuddy.Controllers
{
    [ApiController]
[Route("api/[controller]")]
    public class ImgurController : BaseController
    {
        private readonly DBContext _context;
        private readonly ImgurService _imgurService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<BaseController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeZoneService _timezoneService;

        public ImgurController(
            DBContext context,
            ImgurService imgurService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BaseController> logger,
            IMemoryCache memoryCache,
            TimeZoneService timezoneService
        ) : base(userManager, signInManager, context, logger, memoryCache, timezoneService)
        {
            _context = context;
            _imgurService = imgurService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _memoryCache = memoryCache;
            _timezoneService = timezoneService;
        }

        /*
        [HttpPost]
        public async Task<IActionResult> FriendshipStatus(ApplicationUser target) {
            
            

            return false;
        }*/

        [HttpGet("GetDashboardMessages")]
        public async Task<IActionResult> GetDashboadMessages([FromQuery] int type) {
            try {
                var response = await getDBoardMessages(type);
                return Ok(response);
            } catch (Exception ex) {
                return StatusCode(500, new { error = "An error occurred while fetching messages.", details = ex.Message });
            }
        }

        [HttpPost("SendDashboardMessage")]
        public async Task SendDashboardMessage([FromQuery] int type, [FromQuery]  string message)
        {
            await sendDBoardMessage(type, message);
        }


        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                TempData["Message"] = "No image uploaded.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null)
            {
                TempData["Message"] = "Profile not found.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Upload image to Imgur
            string imageUrl;
            try
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                imageUrl = await _imgurService.UploadImageAsync(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Imgur for user {UserId}", profile.UserId);
                TempData["Message"] = "Failed to upload image. Please try again.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Update the profile image
            try
            {
                EnsureProfileAttached(profile);

                profile.ProfileImageUrl = imageUrl;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Clear and optionally refresh cache
                RevalidateCache(profile);

                TempData["Message"] = "Image uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating profile image for user {UserId}", profile.UserId);
                TempData["Message"] = "Failed to update profile. Please try again.";
            }

            return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateBio(string text, UserProfile model)
        {
            if (string.IsNullOrWhiteSpace(model.Bio))
            {
                TempData["Message"] = "Bio cannot be empty!";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null)
            {
                TempData["Message"] = "Profile not found.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Update bio
            
                EnsureProfileAttached(profile);

                profile.Bio = model.Bio;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Clear and optionally refresh cache
                RevalidateCache(profile);

                TempData["Message"] = "Image uploaded successfully.";


            return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSocialMedia(UserProfile model) {

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null)
            {
                TempData["Message"] = "Profile not found.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Update bio
            
                EnsureProfileAttached(profile);

                if (!string.IsNullOrEmpty(model.FacebookUrl)) {
                    profile.FacebookUrl = model.FacebookUrl;
                }
                if (!string.IsNullOrEmpty(model.TwitterUrl)) {
                    profile.TwitterUrl = model.TwitterUrl;
                }
                if (!string.IsNullOrEmpty(model.InstagramUrl)) {
                    profile.InstagramUrl = model.InstagramUrl;
                }
                if (!string.IsNullOrEmpty(model.LinkedInUrl)) {
                    profile.LinkedInUrl = model.LinkedInUrl;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Clear and optionally refresh cache
                RevalidateCache(profile);

                TempData["Message"] = "Social media links updated successfully.";


            return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateAccountInformation(UserProfile model) {

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null)
            {
                TempData["Message"] = "Profile not found.";
                return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
            }

            // Update bio
            
            EnsureProfileAttached(profile);

            // visibility of profile content if not considered a friend
            Console.WriteLine(profile.ProfileVisibleToPublic);
            if (model.ProfileVisibleToPublic == true) {
                profile.ProfileVisibleToPublic = true;
            } else {
                profile.ProfileVisibleToPublic = false;
            }


            if (!string.IsNullOrEmpty(model.JobTitle)) {
                profile.TwitterUrl = model.TwitterUrl;
            }
            if (!string.IsNullOrEmpty(model.CompanyName)) {
                profile.CompanyName = model.CompanyName;
            }
            if (!string.IsNullOrEmpty(model.SchoolName)) {
                profile.SchoolName = model.SchoolName;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Clear and optionally refresh cache
            RevalidateCache(profile);

            TempData["Message"] = "Profile information has been updated successfully.";


            return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVisibility(UserProfile model) {

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null)
            {
                TempData["Message"] = "Profile not found.";
                return RedirectToAction("Settings", "Account", new { page = "PrivacySettings" });
            }

            // Update bio
            
            EnsureProfileAttached(profile);

            // visibility of profile content if not considered a friend
            profile.ProfileVisibleToPublic = model.ProfileVisibleToPublic;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Clear and optionally refresh cache
            RevalidateCache(profile);

            TempData["Message"] = "Profile account visibility has been updated successfully.";


            return RedirectToAction("Settings", "Account", new { page = "PrivacySettings" });
        }

    }
}
