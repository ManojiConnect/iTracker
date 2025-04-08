using Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Application.Services;
public class SendGridService : IMailService
{
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;

    public SendGridService(string apiKey, IConfiguration configuration)
    {
        _apiKey = apiKey;
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string plainTextContent, string htmlContent)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var fromEmail = _configuration.GetSection("SendGridSettings:FromEmail").Value!;
            var from = new EmailAddress(fromEmail);
            var toAddress = new EmailAddress(to);

            var message = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent);

            try
            {
                var response = await new SendGridClient(_apiKey).SendEmailAsync(message);
                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception)
            {
                // Handle exception, log errors, etc.
                return false;
            }
        }
        return true;
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, string templateId, object templateData)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var fromEmail = _configuration.GetSection("SendGridSettings:FromEmail").Value!;
            var from = new EmailAddress(fromEmail);
            var toAddress = new EmailAddress(to);

            var message = MailHelper.CreateSingleTemplateEmail(from, toAddress, templateId, templateData);

            try
            {
                var response = await new SendGridClient(_apiKey).SendEmailAsync(message);
                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception)
            {
                // Handle exception, log errors, etc.
                return false;
            }
        }
        return true;
    }
}
