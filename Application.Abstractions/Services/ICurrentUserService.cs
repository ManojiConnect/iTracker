namespace Application.Abstractions.Services;

public interface ICurrentUserService
{
    string Id { get; }
    string UserId { get; }
    string Email { get; }
    string Role { get; }
    string FirstName { get; }
    string LastName { get; }
} 