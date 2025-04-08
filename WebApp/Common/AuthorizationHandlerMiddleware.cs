using Application.Abstractions.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;

namespace WebApp.Common;

public class AuthorizationHandlerMiddleware : IMiddleware
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;

    public AuthorizationHandlerMiddleware(
        IContext context,
        UserManager<Infrastructure.Identity.ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Auth/Login");
            }
        }
        await next(context);
    }
}
