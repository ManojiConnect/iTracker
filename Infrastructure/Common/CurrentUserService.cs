using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Common;

public class CurrentUserService : Application.Common.Interfaces.ICurrentUserService
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
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
