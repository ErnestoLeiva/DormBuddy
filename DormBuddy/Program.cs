using Microsoft.EntityFrameworkCore;
using DormBuddy.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DBContext>(options => 
options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
new MySqlServerVersion(new Version(8, 0, 2))));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<DBContext>().AddDefaultTokenProviders();

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.Run();
