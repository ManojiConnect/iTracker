using Application.Features.Auth.Authenticate;
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

/// <summary>
/// Handles user authentication requests. This handler is responsible for validating user credentials,
/// managing authentication cookies, and setting up user claims for authorization.
/// </summary>
public class AuthenticateHandler : IRequestHandler<AuthenticateRequest, Result<AuthenticateResponse>>
{
    private readonly ILogger<AuthenticateHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly SignInManager<Infrastructure.Identity.ApplicationUser> _signInManager;
    private readonly IContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the AuthenticateHandler with required dependencies.
    /// </summary>
    /// <param name="logger">Logger for recording authentication events and errors.</param>
    /// <param name="configuration">Application configuration for authentication settings.</param>
    /// <param name="userManager">Identity user manager for user operations.</param>
    /// <param name="signInManager">Identity sign-in manager for authentication operations.</param>
    /// <param name="context">Database context for data access.</param>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    /// <param name="mediator">Mediator for handling additional requests.</param>
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

    /// <summary>
    /// Handles the authentication request by validating user credentials and setting up authentication.
    /// </summary>
    /// <param name="request">The authentication request containing user credentials.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing either:
    /// - Success: AuthenticateResponse with user details if authentication succeeds
    /// - Error: Error message if authentication fails (invalid credentials, inactive account, etc.)
    /// </returns>
    public async Task<Result<AuthenticateResponse>> Handle(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Error("Invalid email or password.");
        }

        // Check if user account is active
        if (!user.IsActive)
        {
            return Result.Error("User account is inactive.");
        }

        // Attempt to sign in with provided credentials
        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);
        if (!result.Succeeded)
        {
            return Result.Error("Invalid email or password.");
        }

        // Create claims for the authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        // Add user roles to claims
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Create authentication ticket
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            AllowRefresh = true,
            RedirectUri = "/Portfolios/Index"
        };

        // Sign in the user and create authentication cookie
        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Verify the authentication cookie was set
        var cookie = _httpContextAccessor.HttpContext!.Response.Headers["Set-Cookie"];
        if (!cookie.Any(c => c.Contains("AuthCookie")))
        {
            return Result.Error("Authentication cookie was not set");
        }

        // Return successful authentication response
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