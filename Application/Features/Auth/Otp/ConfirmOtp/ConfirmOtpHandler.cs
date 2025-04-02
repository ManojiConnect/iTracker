using Ardalis.Result;
using Application.Services;
using Infrastructure.Context;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Features.Auth.Otp.ConfirmOtp;
public class ConfirmOtpHandler : IRequestHandler<ConfirmOtpRequest, Result<string>>
{
    private readonly OtpService _otpService;
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmOtpHandler(OtpService otpService, IContext context, UserManager<ApplicationUser> userManager)
    {
        _otpService = otpService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(ConfirmOtpRequest request, CancellationToken cancellationToken)
    {
        //get the identity user 
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.NotFound();
        }

        //Confirm the OTP sent by user
        var validateOtp = _otpService.ValidateOtp(request.Email, request.Otp);
        if (!validateOtp)
        {
            return Result.Invalid(new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = $"{nameof(request.Otp)}|{nameof(request.Email)}",
                    ErrorMessage = "Otp is incorrect"
                }
            });
        }

        //return the token
        return await _userManager.GeneratePasswordResetTokenAsync(user);

    }
}
