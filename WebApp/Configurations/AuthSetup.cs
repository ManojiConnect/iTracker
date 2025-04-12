using Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Context;
using Infrastructure.Identity;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using WebApp.Services;
using Microsoft.Extensions.Options;

namespace WebApp.Configurations;

public static class AuthSetup
{
    public static IServiceCollection AddAuthSetup(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<Infrastructure.Identity.ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 1;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // Basic cookie settings
            options.Cookie.Name = "AuthCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = SameSiteMode.Lax;

            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/common/pagenotfound";
            options.ReturnUrlParameter = "returnUrl";
            
            // Default session timeout (will be overridden by middleware)
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            
            // Enable sliding expiration to create new cookie
            options.SlidingExpiration = true;

            // This will enforce cookie renewal
            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var newIdentity = new ClaimsIdentity(identity.Claims, identity.AuthenticationType);
                        context.Principal = new ClaimsPrincipal(newIdentity);
                    }
                },
                OnRedirectToLogin = async context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        // Check if this is a redirect due to session expiration
                        bool isTimeout = context.Response.StatusCode == StatusCodes.Status401Unauthorized && 
                                         context.Request.Cookies.ContainsKey(".AspNetCore.Cookies") &&
                                         !context.HttpContext.User.Identity.IsAuthenticated;
                        
                        // Add timeout parameter only for session timeout
                        var redirectUri = context.RedirectUri;
                        if (isTimeout && !redirectUri.Contains("timeout=true"))
                        {
                            redirectUri = redirectUri.Contains('?') 
                                ? $"{redirectUri}&timeout=true" 
                                : $"{redirectUri}?timeout=true";
                        }
                        context.Response.Redirect(redirectUri);
                    }
                    await Task.CompletedTask;
                }
            };
        });

        // Add session timeout middleware
        services.AddTransient<CookieAuthenticationSessionTimeoutMiddleware>();

        return services;
    }
}

/// <summary>
/// Middleware to apply session timeout from settings
/// </summary>
public class CookieAuthenticationSessionTimeoutMiddleware : IMiddleware
{
    private readonly IApplicationSettingsService _settingsService;
    private readonly ILogger<CookieAuthenticationSessionTimeoutMiddleware> _logger;

    public CookieAuthenticationSessionTimeoutMiddleware(
        IApplicationSettingsService settingsService,
        ILogger<CookieAuthenticationSessionTimeoutMiddleware> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            // Get the current user's auth ticket
            var ticket = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (ticket?.Principal != null)
            {
                // Get the session timeout from settings
                var settings = await _settingsService.GetSettingsAsync();
                
                // Validate timeout value
                if (settings.SessionTimeoutMinutes < 5 || settings.SessionTimeoutMinutes > 120)
                {
                    _logger.LogWarning("Invalid session timeout setting: {Timeout}. Using default of 30 minutes.", 
                        settings.SessionTimeoutMinutes);
                    settings.SessionTimeoutMinutes = 30;
                }
                
                _logger.LogDebug("Setting session timeout to {Timeout} minutes", settings.SessionTimeoutMinutes);

                // Refresh the cookie with the current session timeout setting
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ticket.Principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = ticket.Properties.IsPersistent,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(settings.SessionTimeoutMinutes),
                        AllowRefresh = true,
                        IssuedUtc = ticket.Properties.IssuedUtc,
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying session timeout");
        }
        
        await next(context);
    }
}
