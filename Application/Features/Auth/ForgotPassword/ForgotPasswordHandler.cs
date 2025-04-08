using Ardalis.Result;
using Application.Abstractions.Data;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Application.Common.Interfaces;
using Infrastructure.Identity;
using Application.Abstractions.Services;
namespace Application.Features.Auth.ForgotPassword;
public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IContext context,
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        IMailService mailService,
        IConfiguration configuration,
        IMediator mediator,
        ILogger<ForgotPasswordHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _mailService = mailService;
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Invalid(new[] { new ValidationError("Invalid", "Invalid email address.") });
        }

        if (!user.IsActive)
        {
            return Result.Invalid(new[] { new ValidationError("Invalid", "Your account is inactive. Please contact your administrator.") });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_configuration["AppUrl"]}/Auth/ResetPassword?email={request.Email}&token={token}";

        var emailResult = await _mailService.SendEmailAsync(
            request.Email,
            "Reset Password",
            $"Please click the following link to reset your password: {resetLink}",
            $"<p>Please click the following link to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>");

        if (!emailResult)
        {
            return Result.Invalid(new[] { new ValidationError("Invalid", "Failed to send reset password email.") });
        }

        return Result.Success(true);
    }
}