using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;

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
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ActivityReportService>();


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
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("es") };

    options.DefaultRequestCulture = new RequestCulture("en");
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
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DBContext>(options => 
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
    new MySqlServerVersion(new Version(8, 0, 2))));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

#region PASSWORD REQ.
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});
#endregion

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout (e.g., 30 minutes)
    options.Cookie.HttpOnly = true;  // Make the session cookie HTTP-only for security
    options.Cookie.IsEssential = true;  // Ensure the session cookie is essential (GDPR compliance)
});

#region ACCESS DENIED HANDLING
// Add Authentication/Authorization Middleware and configure access denied handling
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect to this path if access is denied
});

builder.Services.AddAuthorization(options =>
{
    // Allow access to Admin for everything
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));

    // Moderator policy - can access certain resources
    options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole("Moderator", "Admin"));

    // User policy - can access some resources
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Moderator", "Admin"));
});

var app = builder.Build();
#endregion

// Configure the HTTP request pipeline.
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

// Authentication and Authorization middleware
app.UseAuthentication(); // Make sure to add this line
app.UseAuthorization();

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=AccountForms}/{id?}",
    defaults: new { controller = "Account" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomeLogin}/{id?}");

await InitializeRolesAndAdminUser(app);

app.Run();

#region ROLES

static async Task InitializeRolesAndAdminUser(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { Roles.Admin, Roles.Moderator, Roles.User };

        foreach (var role in roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create a default admin account
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@dormbuddy.com",
            FirstName = "Admin",
            LastName = "",
            Credits = 0
        };

        string password = "Adminpass123!";

        var user = await userManager.FindByEmailAsync(adminUser.Email);

        if (user == null)
        {
            var createUserResult = await userManager.CreateAsync(adminUser, password);
            if (createUserResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                Console.WriteLine("Admin account created successfully!");
            }
            else
            {
                Console.WriteLine("Error creating admin account!");
            }
        }
        else
        {
            Console.WriteLine("Admin account already exists!");
        }
    }
}
#endregion