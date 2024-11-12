using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Linq;          
using System.Security.Claims;

namespace DormBuddy.Controllers {


    public class ImgurController : Controller
    {
        private readonly DBContext _context;
        private readonly ImgurService _imgurService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ImgurController(DBContext context, ImgurService imgurService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _imgurService = imgurService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var user = await _userManager.GetUserAsync(User);

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageUrl = await _imgurService.UploadImageAsync(memoryStream.ToArray());

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    ProfileImageUrl = imageUrl
                };

                await _context.UserProfiles.AddAsync(profile);
                await _context.SaveChangesAsync();
            }

            profile.ProfileImageUrl = imageUrl;

            // update navbar image

            await UpdateProfileImageClaim(user, imageUrl);

            var result = await _userManager.UpdateAsync(user);

            //return Ok(new { link = imageUrl });
            if (result.Succeeded) {
                TempData["Message"] = "Success uploading image";
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings" });

            } else {
                TempData["Message"] = "Failure uploading image";
                return RedirectToAction("Settings", "Account");
            }
        }

        
        // Method to update only the ProfileImageUrl claim
        public async Task UpdateProfileImageClaim(ApplicationUser user, string newProfileImageUrl)
        {
            // Retrieve the existing claims for the user
            var existingClaims = await _userManager.GetClaimsAsync(user);

            // Find the existing ProfileImageUrl claim
            var existingProfileImageUrlClaim = existingClaims.FirstOrDefault(c => c.Type == "ProfileImageUrl");

            if (existingProfileImageUrlClaim != null)
            {
                // Remove the existing ProfileImageUrl claim
                await _userManager.RemoveClaimAsync(user, existingProfileImageUrlClaim);
            }

            // Add the new ProfileImageUrl claim
            var newProfileImageUrlClaim = new Claim("ProfileImageUrl", newProfileImageUrl);
            await _userManager.AddClaimAsync(user, newProfileImageUrlClaim);

            // Optional: Refresh sign-in to apply updated claims to the current session
            await _signInManager.RefreshSignInAsync(user);
        }




    }


}