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

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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

        // Seed users will be handled by ModelBuilder in OnModelCreating method of AppDbContext
        // No need to call it explicitly here
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

// Add debug middleware to log route information
app.Use(async (context, next) => {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request Path: {context.Request.Path}, Method: {context.Request.Method}");
    await next.Invoke();
});

app.MapRazorPages();

app.Run();
