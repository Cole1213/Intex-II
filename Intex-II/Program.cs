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

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<IntexIiContext>();


// var services = builder.Services;
// var configuration = builder.Configuration;
//
// services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
// {
//     microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
//     microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
// });


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


app.Run();
