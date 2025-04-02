using Ardalis.Result;
using Application.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Infrastructure.Context;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Entities;
namespace Application.Features.Auth.ForgotPassword;
public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public ForgotPasswordHandler(
        IContext context,
        UserManager<ApplicationUser> userManager,
        IMailService mailService,
        IConfiguration configuration,
        IMediator mediator)
    {
        _context = context;
        _userManager = userManager;
        _mailService = mailService;
        _configuration = configuration;
        _mediator = mediator;
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