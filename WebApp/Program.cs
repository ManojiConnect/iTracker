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
builder.Services.AddPersistence(builder.Configuration, builder.Environment);

// Middleware
builder.Services.AddScoped<ExceptionHandlerMiddleware>();
// builder.Services.AddScoped<RequestAuthorizerMiddleware>();
builder.Services.AddScoped<AuthorizationHandlerMiddleware>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WebApp.Services.CurrencyFormatterService>();
builder.Services.AddScoped<WebApp.Services.IApplicationSettingsService, WebApp.Services.ApplicationSettingsService>();

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
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Get the DbContext
        var dbContext = services.GetRequiredService<AppDbContext>();

        // Ensure database is created
        logger.LogInformation("Ensuring database is created...");
        await dbContext.Database.EnsureCreatedAsync();

        // Commenting out migrations to prevent automatic migration during deployment
        // logger.LogInformation("Applying migrations...");
        // await dbContext.Database.MigrateAsync();
        // logger.LogInformation("Database migrations completed.");

        // Initialize database with roles and admin user
        var databaseInitializer = services.GetRequiredService<DatabaseInitializer>();
        await databaseInitializer.InitializeAsync();
        logger.LogInformation("Database initialization completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while setting up the databases.");
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
