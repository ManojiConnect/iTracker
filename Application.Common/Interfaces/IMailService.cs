using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IMailService
{
    Task<bool> SendEmailAsync(string to, string subject, string plainTextContent, string htmlContent);
    Task<bool> SendEmailWithTemplateAsync(string to, string templateId, object templateData);
} 