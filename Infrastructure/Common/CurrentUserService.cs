using Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Common;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
        if (_httpContextAccessor?.HttpContext?.User != null)
        {
            UserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId") ?? string.Empty;
            Email = _httpContextAccessor.HttpContext.User.FindFirstValue("Email") ?? string.Empty;
            Role = _httpContextAccessor.HttpContext.User.FindFirstValue("Role") ?? string.Empty;
            FirstName = _httpContextAccessor.HttpContext.User.FindFirstValue("FirstName") ?? string.Empty;
            LastName = _httpContextAccessor.HttpContext.User.FindFirstValue("LastName") ?? string.Empty;
        }
    }

    public string? Id => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
}
