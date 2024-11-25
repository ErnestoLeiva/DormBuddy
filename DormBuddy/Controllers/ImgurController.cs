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
        private readonly IConfiguration _configuration;

        public ImgurController(
            DBContext context,
            ImgurService imgurService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<BaseController> logger,
            IMemoryCache memoryCache,
            TimeZoneService timezoneService,
            IConfiguration configuration
        ) : base(userManager, signInManager, context, logger, memoryCache, timezoneService, configuration)
        {
            _context = context;
            _imgurService = imgurService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _memoryCache = memoryCache;
            _timezoneService = timezoneService;
            _configuration = configuration;
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

        [HttpGet("AddFriend")]
        public async Task<IActionResult> AddFriend(string target) {
            
            var targetUser = await _context.Users.FirstOrDefaultAsync(p => p.UserName == target);

            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == targetUser.Id) || (m.UserId == targetUser.Id && m.FriendId == user.Id));

            if (friendsAlready != null)
            {
                return BadRequest(new { error = "You are already friends or the friend request exists." });
            }

            await fmodel.AddAsync(new FriendsModel {
                UserId = user.Id,
                FriendId = targetUser.Id,
                blocked = false,
                pending = true
            });

            await _context.SaveChangesAsync();
            
            return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });

        }

        [HttpGet("RemoveFriend")]
        public async Task<IActionResult> RemoveFriend(string target) {

            var targetUser = await _context.Users.FirstOrDefaultAsync(p => p.UserName == target);

            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == targetUser.Id) || (m.UserId == targetUser.Id && m.FriendId == user.Id));

            if (friendsAlready == null)
            {
                return BadRequest(new { error = "You are not friends currently!" });
            }
            
            if (friendsAlready.blocked == true) {
                return BadRequest(new { error ="You must first unblock this user!" });
            }

            fmodel.Remove(friendsAlready);

            await _context.SaveChangesAsync();
            
            return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });

        }

        [HttpGet("BlockUser")]
        public async Task<IActionResult> BlockUser(string target) {
            
            var targetUser = await _context.Users.FirstOrDefaultAsync(p => p.UserName == target);

            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == targetUser.Id) || (m.UserId == targetUser.Id && m.FriendId == user.Id));

            if (friendsAlready != null && friendsAlready.blocked == true)
            {

                if (friendsAlready.FriendId == targetUser.Id) {
                    return BadRequest(new { error = "You have already blocked this user!" });
                } else {
                    return BadRequest(new { error = "This user already has you blocked!" });
                }
                
            }

            if (friendsAlready != null && friendsAlready.blocked == false) {
                if (friendsAlready.FriendId == targetUser.Id) { // current user blocking friend
                    friendsAlready.blocked = true;
                    
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });

                } else {
                    
                    // Must switch FriendId to UserId and then block

                    var currentUserId = friendsAlready.UserId;
                    var friendId = friendsAlready.FriendId;

                    friendsAlready.UserId = friendId;
                    friendsAlready.FriendId = currentUserId;
                    friendsAlready.blocked = true;

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
                }
            }

            await fmodel.AddAsync(new FriendsModel {
                UserId = user.Id,
                FriendId = targetUser.Id,
                blocked = true
            });

            await _context.SaveChangesAsync();
            
            return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });

        }

        [HttpGet("UnblockUser")]
        public async Task<IActionResult> UnblockUser(string target) {

            var targetUser = await _context.Users.FirstOrDefaultAsync(p => p.UserName == target);

            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Id))
            {
                return BadRequest(new { error = "Invalid target user." });
            }

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == targetUser.Id) || (m.UserId == targetUser.Id && m.FriendId == user.Id));

            if (friendsAlready == null) {
                return BadRequest(new { error = "User is not blocked!" });
            }

            if (friendsAlready != null && friendsAlready.blocked == false && friendsAlready.UserId == user.Id)
            {
                return BadRequest(new { error = "You do not have the user blocked currently!" });
            }
            
            if (friendsAlready.blocked == true) {
                //return BadRequest(new { error ="You must first unblock this user!" });

                fmodel.Remove(friendsAlready);

                await _context.SaveChangesAsync();

                //return Ok(new { status = "Friend removed successfully!" });
                return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
                
            }

            
            
            return Ok(new { error = "Unknown error unblocking user!" });

        }

        [HttpGet("AcceptOrDenyPending")]
        public async Task<IActionResult> AcceptOrDenyPending(string target, string type) { // view pending(to accept) or remove pending(to abort)
            var targetUser = await _context.Users.FirstOrDefaultAsync(p => p.UserName == target);

            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Id))
            {
                return BadRequest(new { error = "Invalid target user." }); // found no user for username
            }

            var user = await GetCurrentUserAsync(); // current user

            if (user == null)
            {
                return Unauthorized(new { error = "User is not logged in." });
            }
            
            var fmodel = _context.FriendsModel;

            //var friendsAlready = await fmodel.FirstOrDefaultAsync(m => (m.UserId == user.Id && m.FriendId == targetUser.Id) || (m.UserId == targetUser.Id && m.FriendId == user.Id));

            // acccept or deny(UserId = target)
            var friendAOrD = await fmodel.FirstOrDefaultAsync(m => m.UserId == targetUser.Id && m.FriendId == user.Id);

            if (friendAOrD == null) { // No friend request have been sent to current user, check bi dir.

                // Cancel request or Block user(UserId = currentUser)
                var friendCOrB = await fmodel.FirstOrDefaultAsync(m => m.UserId == user.Id && m.FriendId == targetUser.Id);

                if (type == "cancel") {
                    _context.Remove(friendCOrB);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
            } else {
                Console.WriteLine("NOT NULL");
                switch (type)
                {
                    case "accept":
                        friendAOrD.pending = false;
                        await _context.SaveChangesAsync();
                    break;
                    case "deny":
                        _context.Remove(friendAOrD);
                        await _context.SaveChangesAsync();
                    break;
                    default:
                        BadRequest(new { error = "Neither 'accept' nor 'deny' has been passed as the type." });
                    break;
                }

                return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
            }

            return BadRequest(new { error = "No friend request found to process." });
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


        public enum ImageType 
        {
            Banner,
            Profile
        };


        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image, ImageType type)
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

                switch (type)
                {
                    case ImageType.Banner:
                        profile.BannerImageUrl = imageUrl;
                    break;
                    case ImageType.Profile:
                        profile.ProfileImageUrl = imageUrl;
                    break;
                    default:
                        return BadRequest( new { error = "No image type found!" } );
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Clear and optionally refresh cache
                RevalidateCache(profile);

                TempData["Message"] = "Image uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating image for user {UserId}", profile.UserId);
                TempData["Message"] = "Failed to update profile. Please try again.";
            }

            return RedirectToAction("Settings", "Account", new { page = "ProfileSettings" });
        }


        [HttpPost("UpdateBio")]
        public async Task<IActionResult> UpdateBio( [FromForm] UserProfile model)
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
