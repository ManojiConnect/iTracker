using Application.Abstractions.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace WebApp.Common;

public class AuthorizationHandlerMiddleware : IMiddleware
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly ILogger<AuthorizationHandlerMiddleware> _logger;

    public AuthorizationHandlerMiddleware(
        IContext context,
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        ILogger<AuthorizationHandlerMiddleware> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip check for login and logout pages
        if (context.Request.Path.StartsWithSegments("/Auth/Login") || 
            context.Request.Path.StartsWithSegments("/Auth/Logout"))
        {
            await next(context);
            return;
        }

        var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Inactive user {Email} attempted to access {Path}", email, context.Request.Path);
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Auth/Login");
                return;
            }
        }
        await next(context);
    }
}
