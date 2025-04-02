using Ardalis.Result;
using MediatR;

namespace Application.Features.Auth.Authenticate;

public class AuthenticateRequest : IRequest<Result<AuthenticateResponse>>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; }
}