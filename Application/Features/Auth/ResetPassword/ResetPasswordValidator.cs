using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.ResetPassword;
public class ResetPasswordValidator:AbstractValidator<ResetPasswordRequest>
{
    private const string PasswordRequirements = "Password must be at least 10 characters long and include one uppercase letter, one lowercase letter, and one number.";
    public ResetPasswordValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Must(password =>
                password != null &&
                password.Length >= 10 &&
                password.Any(char.IsUpper) &&
                password.Any(char.IsLower) &&
                password.Any(char.IsDigit))
            .WithMessage(PasswordRequirements);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
