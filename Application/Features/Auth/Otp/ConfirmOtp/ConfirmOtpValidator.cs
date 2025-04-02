using FluentValidation;

namespace Application.Features.Auth.Otp.ConfirmOtp;
public class ConfirmOtpValidator : AbstractValidator<ConfirmOtpRequest>
{
    public ConfirmOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
