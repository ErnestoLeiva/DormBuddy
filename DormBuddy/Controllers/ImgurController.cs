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
using FirebaseAdmin.Messaging;

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

        [HttpPost("UppdateEUFLInformation")]
        public async Task<IActionResult> UpdateEUFLInformation([FromForm] UserProfile model, [FromForm] string Email, [FromForm] string Username, [FromForm] string FirstName, [FromForm] string LastName) {

            // Get the tracked user profile
            var profile = await GetUserInformation();
            if (profile == null || profile.User == null)
            {
                TempData["Message"] = "Profile and/or user not found.";
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings" });
            }

            if (!string.IsNullOrEmpty(Email) && Email != profile?.User?.Email) {
                if (profile?.User != null)
                {
                    profile.User.Email = Email;
                    profile.User.NormalizedEmail = Email.ToUpper();
                }

                // send email to check for new email verification
            }
            if (!string.IsNullOrEmpty(Username) && Username != profile?.User?.UserName) {
                if (profile?.User != null)
                {
                    profile.User.UserName = Username;
                    profile.User.NormalizedUserName = Username.ToUpper();
                }
            }
            if (!string.IsNullOrEmpty(FirstName) && FirstName != profile?.User?.FirstName) {
                if (profile?.User != null)
                {
                    profile.User.FirstName = FirstName;
                }
            }
            if (!string.IsNullOrEmpty(LastName) && LastName != profile?.User?.LastName) {
                if (profile?.User != null)
                {
                    profile.User.LastName = LastName;
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Clear and optionally refresh cache
            if (profile != null) {
                RevalidateCache(profile);
            }

            TempData["Message"] = "Account information has been updated successfully.";


            return RedirectToAction("Settings", "Account", new { page = "AccountSettings" });

        }

        [HttpPost("UpdateAccountPassword")]
        public async Task<IActionResult> UpdateAccountPassword([FromForm] UserProfile model, [FromForm] string NewPassword, [FromForm] string ReEnteredPassword, [FromForm] string OldPassword)
        {


            if (string.IsNullOrEmpty(NewPassword) ||
            string.IsNullOrEmpty(ReEnteredPassword) ||
            string.IsNullOrEmpty(OldPassword))
            {
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = "One or more fields are empty!" });
                
            }

            if (NewPassword != ReEnteredPassword)
            {
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = "New and re-entered passwords do not match!" });
            }

            // check if passwords match parameters

            var userName = User.Identity?.Name ?? string.Empty;  // Use an empty string as fallback
            var user = await _context.Users.FirstOrDefaultAsync(p => p.UserName == userName);

            if (user == null) {
                return BadRequest( new { error = "User not found!" } );
            }

            if (!(await _userManager.CheckPasswordAsync(user, OldPassword)))
            {
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = "Old password is not correct!" });
            }

            if (OldPassword == NewPassword)
            {
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = "New password must be different from the old one!" });
            }

            // change password

            var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);

            try {

            if ( model == null || model.User == null )
            {
                UserProfile up = await GetUserInformation(user.UserName);

                if (up == null)
                {
                    return BadRequest(new { error = "Failed to fetch user model" });
                }

                model = up;
            }

            if (!result.Succeeded) {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                ModelState.AddModelError(string.Empty, errors);

                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = errors });
                
            } else {
                return RedirectToAction("Settings", "Account", new { page = "AccountSettings", errorMessage = "Password changed successfully!" });
            }

            //// send email confirming password change

            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return BadRequest(new{Error = "Failed to reset password!"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllFriends() {
            var user = await GetCurrentUserAsync();

            if (user == null) {
                return BadRequest("Current user could not be found!");
            }

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

            // Check if the notification already exists
            var existingNotification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.UserId == user.Id && n.Message == targetUser.Id && n.MessageType == 2);

            if (existingNotification == null)
            {
                // Notification doesn't exist, so add a new one
                await _context.Notifications.AddAsync(new Notifications
                {
                    UserId = targetUser.Id,
                    Message = user.Id,
                    MessageType = 2,
                    CreatedAt = DateTime.UtcNow
                });
            }


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
            
            if (friendsAlready?.blocked == true) {
                //return BadRequest(new { error ="You must first unblock this user!" });

                fmodel.Remove(friendsAlready);

                await _context.SaveChangesAsync();

                //return Ok(new { status = "Friend removed successfully!" });
                return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
                
            }

            
            
            return Ok(new { error = "Unknown error unblocking user!" });

        }

        [HttpPost("AcceptOrDenyPending")]
        public async Task<IActionResult> AcceptOrDenyPending(string target, string type, string targetCtrl) { // view pending(to accept) or remove pending(to abort)
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
                    if (friendCOrB != null)
                    {
                        _context.Remove(friendCOrB);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
            } else {
                var notification = await _context.Notifications.FirstOrDefaultAsync(p=>p.UserId == friendAOrD.FriendId && p.Message == friendAOrD.UserId && friendAOrD.pending == true);
                
                switch (type)
                {
                    case "accept":
                        friendAOrD.pending = false;

                        try {
                            if (notification != null)
                            {
                                _context.Remove(notification);
                            }
                            Console.WriteLine("notification has been removed");
                        } catch (Exception ex) {Console.WriteLine(ex.Message);}

                        await _context.SaveChangesAsync();
                    break;
                    case "deny":
                        _context.Remove(friendAOrD);

                        try {
                            if (notification != null)
                            {
                                _context.Remove(notification);
                            }
                        } catch (Exception ex) {Console.WriteLine(ex.Message);}

                        await _context.SaveChangesAsync();
                    break;
                    default:
                        BadRequest(new { error = "Neither 'accept' nor 'deny' has been passed as the type." });
                    break;
                }

                switch (targetCtrl) {
                    case "Profile":
                    return RedirectToAction("Profile", "Account", new { username = targetUser.UserName });
                    
                    case "Notification":
                    return RedirectToAction("Index", "Notifications");

                    default:
                    return BadRequest("Controller could not be found!");

                    
                }
            }
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

        [HttpGet]
        public async Task<IActionResult> ProfilePostArea([FromForm] UserProfile model, [FromForm] string message, [FromForm] bool isReply) {
            var u = await GetCurrentUserAsync();

            if (u == null) {
                return BadRequest("Current user could not be found!");
            }

            Console.WriteLine(u.UserName);
            return Ok("true");
        }

    }
}
