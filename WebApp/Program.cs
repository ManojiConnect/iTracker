using Application.Services;
using WebApplication.Common;
using Microsoft.AspNetCore.Builder;
using WebApp.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Application.Extensions;
using FluentValidation;
using Application;
using Microsoft.AspNetCore.Authorization;
using WebApp.Common;
using Infrastructure.Configurations;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication.Configurations;
using Infrastructure;
using WebApp.Services;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Add Views as a location for finding Razor views
        options.ViewLocationFormats.Add("/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
        options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
    });
// Application layer setup
builder.Services.AddApplication(builder.Configuration);

// Authn / Authrz
builder.Services.AddAuthSetup(builder.Configuration);

// In Program.cs or Startup.cs
var expiryValue = builder.Configuration.GetValue<int>("PasswordResetExpiry");

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    var expiryValue = builder.Configuration.GetValue<int>("PasswordResetExpiry");
    options.TokenLifespan = TimeSpan.FromHours(expiryValue);
});

builder.Services.AddMediatRSetup();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Persistence - use the WebApp persistence setup
builder.Services.AddPersistance(builder.Configuration);

// Middleware
builder.Services.AddScoped<ExceptionHandlerMiddleware>();

builder.Services.AddScoped<AuthorizationHandlerMiddleware>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<UserService>();

// Add database initializer
builder.Services.AddScoped<DatabaseInitializer>();

builder.Logging.ClearProviders();
// Add serilog
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Host.UseLoggingSetup(builder.Configuration);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply migrations for the single context
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        // Initialize database with roles and admin user
        var databaseInitializer = services.GetRequiredService<DatabaseInitializer>();
        await databaseInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.UseMiddleware(typeof(ExceptionHandlerMiddleware));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware(typeof(AuthorizationHandlerMiddleware));
app.UseAuthorization();

// Configure conventional routes for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "userManagement",
    pattern: "UserManagement/{action=Index}/{id?}",
    defaults: new { controller = "UserManagement" });

app.MapRazorPages();

app.Run();
