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
        
        [HttpPost]
        public async Task<IActionResult> FriendshipStatus(ApplicationUser target) {
            var user = await GetCurrentUserAsync();
            var friendTable = await _context.FriendsModel.Where(p => p.UserId == user.Id).ToListAsync();

            var t = friendTable.FirstOrDefault(m => (m.UserId == user.Id && m.FriendId == target.Id) || (m.UserId == target.Id && m.FriendId == user.Id));


            if (t == null) {
                return Ok(new { status = "not friends" });
            }

            if (t.blocked) {
                return Ok(new { status = "blocked" });
            } else {
                return Ok(new { status = "friends" });
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> GetAllFriends() {
            var user = await GetCurrentUserAsync();
            var friendsTable = await _context.FriendsModel.Where(p => (p.UserId == user.Id && p.blocked == false) || (p.FriendId == user.Id && p.blocked == false)).ToListAsync();


            if (friendsTable == null) {
                return BadRequest(new { error = "No friends!" });
            }

            var friendsDetails = new List<object>();

            foreach (var friend in friendsTable)
            {
                // Get the friend user (either UserId or FriendId based on the current user)
                var friendId = friend.UserId == user.Id ? friend.FriendId : friend.UserId;

                var friendUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == friendId);

                if (friendUser != null)
                {
                    friendsDetails.Add(BuildUserProfile(friendUser, null));
                }
            }

            return Ok(new { friends = friendsDetails });
            
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(ApplicationUser target) {

            if (target == null || string.IsNullOrWhiteSpace(target.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == target.Id) || (m.UserId == target.Id && m.FriendId == user.Id));

            if (friendsAlready != null)
            {
                return BadRequest(new { error = "You are already friends or the friend request exists." });
            }

            await fmodel.AddAsync(new FriendsModel {
                UserId = user.Id,
                FriendId = target.Id,
                blocked = false
            });

            await _context.SaveChangesAsync();
            
            return Ok(new { status = "Friend added successfully!" });

        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(ApplicationUser target) {

            if (target == null || string.IsNullOrWhiteSpace(target.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == target.Id) || (m.UserId == target.Id && m.FriendId == user.Id));

            if (friendsAlready == null)
            {
                return BadRequest(new { error = "You are not friends currently!" });
            }
            
            if (friendsAlready.blocked == true) {
                return BadRequest(new { error ="You must first unblock this user!" });
            }

            fmodel.Remove(friendsAlready);

            await _context.SaveChangesAsync();
            
            return Ok(new { status = "Friend removed successfully!" });

        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(ApplicationUser target) {

            if (target == null || string.IsNullOrWhiteSpace(target.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == target.Id) || (m.UserId == target.Id && m.FriendId == user.Id));

            if (friendsAlready != null && friendsAlready.blocked == true)
            {

                if (friendsAlready.FriendId == target.Id) {
                    return BadRequest(new { error = "You have already blocked this user!" });
                } else {
                    return BadRequest(new { error = "This user already has you blocked!" });
                }
                
            }

            if (friendsAlready != null && friendsAlready.blocked == false) {
                if (friendsAlready.FriendId == target.Id) { // current user blocking friend
                    friendsAlready.blocked = true;
                    
                    await _context.SaveChangesAsync();

                    return Ok(new { status = "User has been blocked!" });

                } else {
                    
                    // Must switch FriendId to UserId and then block

                    var currentUserId = friendsAlready.UserId;
                    var friendId = friendsAlready.FriendId;

                    friendsAlready.UserId = friendId;
                    friendsAlready.FriendId = currentUserId;
                    friendsAlready.blocked = true;

                    await _context.SaveChangesAsync();

                    return Ok(new { status = "User has been blocked!" });
                }
            }

            await fmodel.AddAsync(new FriendsModel {
                UserId = user.Id,
                FriendId = target.Id,
                blocked = true
            });

            await _context.SaveChangesAsync();
            
            return Ok(new { status = "User blocked successfully!" });

        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(ApplicationUser target) {

            if (target == null || string.IsNullOrWhiteSpace(target.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == target.Id) || (m.UserId == target.Id && m.FriendId == user.Id));

            if (friendsAlready == null) {
                return BadRequest(new { error = "User is not blocked!" });
            }

            if (friendsAlready != null && friendsAlready.blocked == false && friendsAlready.UserId == user.Id)
            {
                return BadRequest(new { error = "You do not have the user blocked currently!" });
            }
            
            if (friendsAlready.blocked == true && friendsAlready.UserId == user.Id) {
                //return BadRequest(new { error ="You must first unblock this user!" });

                fmodel.Remove(friendsAlready);

                await _context.SaveChangesAsync();

                return Ok(new { status = "Friend removed successfully!" });
            }

            
            
            return Ok(new { error = "Unknown error unblocking user!" });

        }

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
