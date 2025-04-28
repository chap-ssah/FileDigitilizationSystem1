using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FileDigitilizationSystem.Data;
using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using FileDigitilizationSystem.Services;

    var builder = WebApplication.CreateBuilder(args);

// Configure the DbContext with SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Configure identity options here if needed:
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>() // if you're using roles such as "Admin", etc.
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();  // This adds the default UI for Identity.

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

//  Bind SmtpSettings from configuration:
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

//  Register the IEmailSender:
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add controllers with views.
builder.Services.AddControllersWithViews();

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

// Authentication & Authorization Middleware.
app.UseAuthentication();
app.UseAuthorization();

// Set up default routing.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // This exposes the Identity UI endpoints.

// Seed the database (see next step).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await FileDigitilizationSystem.Data.DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
