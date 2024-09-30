using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

#region SERVICES CONFIGURATION
// Add services to the container.
builder.Services.AddControllersWithViews();

#region DATABASE CONFIGURATION
// Database configuration using MySQL
builder.Services.AddDbContext<DBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 2))));
#endregion

#region IDENTITY CONFIGURATION
// Identity configuration for user management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // email confirmation
    options.SignIn.RequireConfirmedEmail = true;

    // lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();
#endregion

#region EMAIL SENDER
builder.Services.AddTransient<DormBuddy.Models.IEmailSender, Smtp>();
#endregion

#region PASSWORD REQUIREMENTS
// Configure password requirements for users
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});
#endregion

#region SESSION CONFIGURATION
// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout to 30 minutes
    options.Cookie.HttpOnly = true;  // Secure cookie
    options.Cookie.IsEssential = true;  // GDPR compliance
});
#endregion

#region AUTHENTICATION AND AUTHORIZATION
// Authentication/Authorization configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect on access denied
});

// Role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole("Moderator", "Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Moderator", "Admin"));
});
#endregion

#endregion

var app = builder.Build();

#region MIDDLEWARE CONFIGURATION
// Configure middleware for HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session handling
app.UseSession();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map default routes
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=AccountForms}/{id?}",
    defaults: new { controller = "Account" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomeLogin}/{id?}");
#endregion

await InitializeRolesAndAdminUser(app);

app.Run();

#region ROLE INITIALIZATION
// Initialize default roles
static async Task InitializeRolesAndAdminUser(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { Roles.Admin, Roles.Moderator, Roles.User };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
#endregion
