using Ardalis.Result;
using Application.Services;
using Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Features.Auth.Otp.SendOtp;
public class SendOtpHandler : IRequestHandler<SendOtpRequest, Result<bool>>
{
    private readonly IMailService _mailService;
    private readonly OtpService _otpService;
    private readonly IContext _context;
    private readonly IConfiguration _configuration;

    public SendOtpHandler(IMailService mailService, OtpService otpService, IContext context, IConfiguration configuration)
    {
        _mailService = mailService;
        _otpService = otpService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(SendOtpRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive == true, cancellationToken);
        if (user is null)
        {
            return Result.NotFound();
        }

        var otp = _otpService.SetOtp(user.Email!);

        //var fromEmail = "saurabh.talele@etevatech.com";
        //var toEmail = request.Email;
        var subject = "Otp";
        //var plainTextContent = otp;
        //var htmlContent = $"<h2>{otp}</h2>";

        var emailData = new
        {
            Otp = otp,
            user.FirstName,
            user.LastName,
        };
        var templateId = _configuration.GetSection($"SendGridSettings:OtpTemplateId:{user.Language ?? "EN"}").Value!;
        if (templateId == null)
            templateId = _configuration.GetSection($"SendGridSettings:OtpTemplateId:EN").Value!;

        var isEmailSent = await _mailService.SendEmailWithTemplateAsync(request.Email, templateId, emailData);
        //var isEmailSent = await _mailService.SendEmailAsync(fromEmail, toEmail, subject, plainTextContent, htmlContent);
        if (isEmailSent)
        {
            return true;
        }

        return false;
    }
}
