using Ardalis.Result;
using MediatR;

namespace Application.Features.Auth.ChangePassword;
public class ChangePasswordRequest : IRequest<Result<bool>>
{
    public string CurrentPasword { get; set; }
    public string NewPassword { get; set; }
}
