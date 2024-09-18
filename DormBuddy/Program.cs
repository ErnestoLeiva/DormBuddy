using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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

app.UseAuthorization();

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