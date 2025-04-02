using MediatR;

namespace Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand : IRequest<bool>
{
    public string Email { get; init; }
    public string Token { get; init; }
    public string Password { get; init; }
} 