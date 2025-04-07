using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Application.Services;
public class SendGridService : IMailService
{
    private readonly SendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;

    public SendGridService(string apiKey, IConfiguration configuration)
    {
        _sendGridClient = new SendGridClient(apiKey);
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string fromEmail, string toEmail, string subject, string plainTextContent)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var from = new EmailAddress(fromEmail);
            var to = new EmailAddress(toEmail);

            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

            try
            {
                var response = await _sendGridClient.SendEmailAsync(message);
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

    public async Task<bool> SendEmailAsync(string fromEmail, string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var from = new EmailAddress(fromEmail);
            var to = new EmailAddress(toEmail);

            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await _sendGridClient.SendEmailAsync(message);
                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception)
            {
                // Handle exception, log errors, etc.
                return false;
            }
        }
        else
            return true;
    }

    public async Task<Response> SendEmailAsync(SendGridMessage message)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var fromEmail = _configuration.GetSection("SendGridSettings:FromEmail").Value!;
            var client = new SendGridClient(_configuration.GetSection("SendGridSettings:ApiKey").Value!);
            return await client.SendEmailAsync(message);
        }
        return new Response(HttpStatusCode.OK, null, null);
    }

    public async Task<bool> SendEmailWithTemplateAsync<T>(string toEmail, string templateId, T data, string? subject)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var message = new SendGridMessage();
            message.SetTemplateId(templateId);

            if (!string.IsNullOrEmpty(subject))
            {
                message.SetSubject(subject);
            }

            message.SetTemplateData(new { Data = data });
            message.AddTo(new EmailAddress(toEmail));

            try
            {
                var response = await SendEmailAsync(message);
                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception)
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public async Task<bool> SendEmailWithTemplateAsync(string toEmail, string templateId, object templateData)
    {
        if (Convert.ToBoolean(_configuration.GetSection("SendGridSettings:SendEmail").Value!))
        {
            var fromEmail = _configuration.GetSection("SendGridSettings:FromEmail").Value!;
            var from = new EmailAddress(fromEmail);
            var to = new EmailAddress(toEmail);

            var message = MailHelper.CreateSingleTemplateEmail(from, to, templateId, templateData);

            try
            {
                var response = await _sendGridClient.SendEmailAsync(message);
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
