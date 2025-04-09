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

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "AuthCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = SameSiteMode.Lax;

            // Get session timeout from settings service
            var settingsService = services.BuildServiceProvider().GetRequiredService<IApplicationSettingsService>();
            var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();
            options.ExpireTimeSpan = TimeSpan.FromMinutes(settings.SessionTimeoutMinutes);

            options.LoginPath = "/Auth/Login";
            options.LogoutPath = "/Auth/Logout";
            options.AccessDeniedPath = "/common/pagenotfound";
            options.ReturnUrlParameter = "returnUrl";
            
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
                        context.Response.Redirect(context.RedirectUri);
                    }
                    await Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
