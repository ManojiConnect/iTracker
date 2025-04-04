using MediatR;

namespace Application.Features.Users.CreateUser;

public record CreateUserCommand : IRequest<bool>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Role { get; init; }
    public string? Language { get; init; }
    public string? ProfileUrl { get; init; }
    public bool IsActive { get; init; } = true;
} 