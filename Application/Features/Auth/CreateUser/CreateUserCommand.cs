using MediatR;

namespace Application.Features.Auth.CreateUser;

public record CreateUserCommand : IRequest<bool>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
} 