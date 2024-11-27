using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

// Set up localization with a specific resource path
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

#region SERVICES CONFIGURATION

// Cookie policy configuration
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax; // Set to Lax for session compatibility
    options.Secure = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<TimeZoneService>();
builder.Services.AddSingleton<ImgurService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<UserLastUpdateActionFilter>();
builder.Services.AddScoped<NavBarInfoService>();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

#region SESSION CONFIGURATION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

builder.Services.AddControllersWithViews(options => 
{
    options.Filters.Add<UserLastUpdateActionFilter>();
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options => 
{

    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("es")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = "Culture"
    });

    options.FallBackToParentCultures = true;
});

// Initialize Firebase
FirebaseApp.Create(new AppOptions() 
{
    Credential = GoogleCredential.FromFile("dormbuddy-33ce0-firebase-adminsdk-5i0gl-c049a0fe9a.json")
});

#region DATABASE CONFIGURATION
builder.Services.AddDbContext<DBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 2)))); 
#endregion

#region IDENTITY CONFIGURATION
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedEmail = true;
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

#region AUTHENTICATION AND AUTHORIZATION
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
} 
else 
{
    Console.WriteLine("Development mode: Active");
}

app.UseCookiePolicy();

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.Use(async (context, next) =>
{
    var cookieValue = context.Request.Cookies["Culture"];
    if (!string.IsNullOrEmpty(cookieValue))
    {
        var culture = new CultureInfo(cookieValue);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // UseSession must be before Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=AccountForms}/{id?}",
    defaults: new { controller = "Account" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomeLogin}/{id?}");
#endregion

// Call InitializeRolesAndAdminUser to create roles and the default admin user
await InitializeRolesAndAdminUser(app);

app.Run();

#region ROLE AND ADMIN INITIALIZATION
static async Task InitializeRolesAndAdminUser(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Define roles
        string[] roles = { Roles.Admin, Roles.Moderator, Roles.User };

        // Create roles if they don't exist
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create the default admin user if it doesn't exist
        var defaultAdminEmail = "admin@dormbuddy.com"; // Use a real email
        var defaultAdminUsername = "admin";
        var defaultAdminPassword = "Adminpass123!"; // Use a secure password

        var adminUser = await userManager.FindByEmailAsync(defaultAdminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = defaultAdminUsername,
                Email = defaultAdminEmail
            };

            var createUserResult = await userManager.CreateAsync(adminUser, defaultAdminPassword);

            if (createUserResult.Succeeded)
            {
                // Assign the Admin role to the user
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
            else
            {
                // Log any errors or handle them as needed
                foreach (var error in createUserResult.Errors)
                {
                    Console.WriteLine($"Error creating admin user: {error.Description}");
                }
            }
        }
    }
}
#endregion
