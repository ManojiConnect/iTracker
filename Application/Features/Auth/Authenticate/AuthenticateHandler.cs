﻿using Application.Features.Auth.Authenticate;
using Application.Features.Common.Responses;
using Application.Services;
using Ardalis.Result;
using Application.Abstractions.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using Application.Common.Interfaces;
using Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Infrastructure.Identity;

namespace Application.Features.Auth.Authenticate;

public class AuthenticateHandler : IRequestHandler<AuthenticateRequest, Result<AuthenticateResponse>>
{
    private readonly ILogger<AuthenticateHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly SignInManager<Infrastructure.Identity.ApplicationUser> _signInManager;
    private readonly IContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public AuthenticateHandler(
        ILogger<AuthenticateHandler> logger,
        IConfiguration configuration,
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        SignInManager<Infrastructure.Identity.ApplicationUser> signInManager,
        IContext context,
        IHttpContextAccessor httpContextAccessor,
        IMediator mediator)
    {
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    public async Task<Result<AuthenticateResponse>> Handle(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Invalid(new[] { new ValidationError("Invalid", "Invalid email or password.") });
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);
        if (!result.Succeeded)
        {
            return Result.Invalid(new[] { new ValidationError("Invalid", "Invalid email or password.") });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            AllowRefresh = true,
            RedirectUri = "/Portfolios/Index"
        };

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Verify the cookie was set
        var cookie = _httpContextAccessor.HttpContext!.Response.Headers["Set-Cookie"];
        if (!cookie.Any(c => c.Contains("AuthCookie")))
        {
            throw new InvalidOperationException("Authentication cookie was not set");
        }

        return Result.Success(new AuthenticateResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Language = user.Language,
            ProfileUrl = user.ProfileUrl,
            IsActive = true
        });
    }
}