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
    public class ImgurController : BaseController
    {
        private readonly DBContext _context;
        private readonly ImgurService _imgurService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<BaseController> _logger;
        private readonly IMemoryCache _memoryCache;

        public ImgurController(
            DBContext context,
            ImgurService imgurService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BaseController> logger,
            IMemoryCache memoryCache
        ) : base(userManager, signInManager, context, logger, memoryCache)
        {
            _context = context;
            _imgurService = imgurService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _memoryCache = memoryCache;
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


    }
}
