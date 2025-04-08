using System.Threading.Tasks;

namespace Application.Abstractions.Services;

public interface IMailService
{
    Task<bool> SendEmailAsync(string to, string subject, string plainTextContent, string htmlContent);
    Task<bool> SendEmailWithTemplateAsync(string to, string templateId, object templateData);
} 