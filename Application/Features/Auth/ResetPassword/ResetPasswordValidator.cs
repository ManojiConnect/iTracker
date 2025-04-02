using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.ResetPassword;
public class ResetPasswordValidator:AbstractValidator<ResetPasswordRequest>
{
    private const string PasswordRequirements = "Password is required";
    public ResetPasswordValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(PasswordRequirements);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
