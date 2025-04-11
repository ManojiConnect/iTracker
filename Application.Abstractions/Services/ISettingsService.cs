using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Abstractions.Services;

public interface ISettingsService
{
    Task<SystemSettings> GetAllSettingsAsync();
    Task<string> GetSettingAsync(string key);
    Task<bool> UpdateSettingsAsync(SystemSettings updatedSettings);
    Task<bool> UpdateSettingAsync(string key, string value);
    void InvalidateCache();
} 