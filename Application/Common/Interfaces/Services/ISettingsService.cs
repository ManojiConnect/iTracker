using Domain.Entities;

namespace Application.Common.Interfaces.Services;

public interface ISettingsService
{
    Task<SystemSettings> GetAllSettingsAsync();
    Task<string> GetSettingAsync(string key);
    Task<bool> UpdateSettingsAsync(SystemSettings updatedSettings);
    Task<bool> UpdateSettingAsync(string key, string value);
} 