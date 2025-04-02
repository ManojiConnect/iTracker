﻿using Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Context;
using Infrastructure.Identity;
using System.Security.Claims;
using System.Text;
using Domain.Entities;

namespace WebApp.Configurations;

public static class AuthSetup
{
    public static IServiceCollection AddAuthSetup(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
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

            // Set cookie expiration to 7 days
            options.ExpireTimeSpan = TimeSpan.FromDays(7);

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
