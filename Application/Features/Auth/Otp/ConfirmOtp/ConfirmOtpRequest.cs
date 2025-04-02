using Ardalis.Result;
using MediatR;

namespace Application.Features.Auth.Otp.ConfirmOtp;
public record ConfirmOtpRequest : IRequest<Result<string>>
{
    public string Email { get; set; }

    public string Otp { get; set; }

}
