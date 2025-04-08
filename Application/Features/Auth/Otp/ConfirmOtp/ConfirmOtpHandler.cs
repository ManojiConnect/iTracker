using Ardalis.Result;
using Application.Abstractions.Data;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Application.Services;
using Infrastructure.Identity;

namespace Application.Features.Auth.Otp.ConfirmOtp;
public class ConfirmOtpHandler : IRequestHandler<ConfirmOtpRequest, Result<string>>
{
    private readonly OtpService _otpService;
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly ILogger<ConfirmOtpHandler> _logger;

    public ConfirmOtpHandler(
        OtpService otpService,
        IContext context, 
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        ILogger<ConfirmOtpHandler> logger)
    {
        _otpService = otpService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ConfirmOtpRequest request, CancellationToken cancellationToken)
    {
        //get the identity user 
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Error("User not found");
        }

        //verify the otp
        var isValid = _otpService.ValidateOtp(request.Email, request.Otp);
        if (!isValid)
        {
            return Result.Error("Invalid OTP");
        }

        //return the token
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
}
