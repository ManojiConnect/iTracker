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

namespace Application.Features.Auth.Otp.SendOtp;
public class SendOtpHandler : IRequestHandler<SendOtpRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly OtpService _otpService;
    private readonly ILogger<SendOtpHandler> _logger;

    public SendOtpHandler(
        IContext context, 
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        OtpService otpService,
        ILogger<SendOtpHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendOtpRequest request, CancellationToken cancellationToken)
    {
        //get the identity user 
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Error("User not found");
        }

        //generate and send otp
        var otp = _otpService.SetOtp(request.Email);
        if (string.IsNullOrEmpty(otp))
        {
            return Result.Error("Failed to generate OTP");
        }

        return Result.Success(true);
    }
}
