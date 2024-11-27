using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class UserLastUpdateActionFilter : IAsyncActionFilter
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DBContext _dbContext;

    public UserLastUpdateActionFilter(UserManager<ApplicationUser> userManager, DBContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the action method
        var resultContext = await next();

        // After the action has executed, check if the user is authenticated
        if (resultContext.HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(resultContext.HttpContext.User);

            if (user != null)
            {
                // Ensure the current user's LastUpdate status is updated only for them.
                await SetUserLastUpdateAsync(user);
            }
        }
    }

    private async Task SetUserLastUpdateAsync(ApplicationUser user)
    {
        try
        {
            // Ensure `UserId` and `LastUpdate` properties are available in `UserLastUpdate`
            var instance = await _dbContext.UserLastUpdate.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            if (instance != null)
            {
                // Only update if more than 60 seconds have passed since the last update
                if ((DateTime.UtcNow - instance.LastUpdate).TotalSeconds > 60) 
                {
                    instance.LastUpdate = DateTime.UtcNow;
                }
            }
            else
            {
                // Add a new entry if no existing record is found for the current user
                _dbContext.UserLastUpdate.Add(new UserLastUpdate
                {
                    UserId = user.Id,
                    LastUpdate = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error updating last login: {ex.Message}");
        }
    }
}
