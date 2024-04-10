using Intex_II.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
// var connectionString = builder.Configuration.GetConnectionString("IdentityDataContextConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDataContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IntexIiContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:LegoConnection"]));


builder.Services.AddRazorPages();  // Add this line to register Razor Pages services

// builder.Services.AddDbContext<IntexIiContext>(options =>
// {
//     options.UseSqlServer(builder.Configuration["ConnectionStrings:LegoConnection"]);
// });

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 2;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
    }).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IntexIiContext>();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = configuration["ClientId"];
    microsoftOptions.ClientSecret = configuration["ClientSecret"];
});


builder.Services.AddScoped<ILegoRepository, EFLegoRepository>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
}
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
    options.ExcludedHosts.Add("example.com");
    options.ExcludedHosts.Add("www.example.com");
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Customer"))
    {
        await roleManager.CreateAsync(new IdentityRole("Customer"));
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        // Allow scripts from your own domain, Google APIs, unpkg.com, Font Awesome, and jQuery
        "script-src 'self' 'unsafe-inline' https://apis.google.com https://unpkg.com https://kit.fontawesome.com https://code.jquery.com; " +
        // Consolidated style-src directive to allow styles from your own domain, Google Fonts, and Font Awesome
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://use.fontawesome.com; " +
        // Allow styles applied via attributes, consider security implications of 'unsafe-inline'
        "style-src-attr 'self' 'unsafe-inline'; " +
        // Allow images from your own domain, trusted CDNs, Amazon media, and LEGO
        "img-src 'self' https://*.cdn.com https://m.media-amazon.com https://www.lego.com; " +
        // Consolidated font-src directive to allow fonts from your own domain, Google Fonts, and Font Awesome
        "font-src 'self' https://fonts.gstatic.com https://use.fontawesome.com";
        // Specify the report-to endpoint for violations
        // "report-uri /Home/LogCspReport;";
    await next();
});

app.Run();
