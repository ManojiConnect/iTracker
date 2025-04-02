using Ardalis.Result;
using MediatR;

namespace Application.Features.Auth.UserExists;
public record UserExistsRequest : IRequest<Result<bool>>
{
    public UserExistsRequest(string email)
    {
        Email = email;
    }

    public string Email { get; init; } = null!;
}
