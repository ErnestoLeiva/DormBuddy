using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

public class NavBarInfoService 
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly DBContext _context;

    private readonly IMemoryCache _memoryCache;

    private readonly IConfiguration _configuration;

    public NavBarInfoService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        DBContext context,
        IMemoryCache memoryCache,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public async Task<UserProfile> GetUserInformationAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        var profile = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id);


        var cacheKey = $"userProfile_{user.Id}";
        if (_memoryCache.TryGetValue(cacheKey, out UserProfile cachedProfile))
        {
            profile = cachedProfile;
        }

        if (user == null || profile == null)
        {
            return null;
        }
        
        if (string.IsNullOrEmpty(profile.ProfileImageUrl)) {
            profile.ProfileImageUrl = _configuration["Profile:Default_ProfileImage"];
        }

        return profile;
    }
}