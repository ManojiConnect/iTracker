using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Gets all system settings
    /// </summary>
    Task<SystemSettings> GetAllSettingsAsync();
    
    /// <summary>
    /// Gets a specific setting by key
    /// </summary>
    /// <param name="key">The setting key</param>
    Task<string> GetSettingAsync(string key);
    
    /// <summary>
    /// Updates system settings
    /// </summary>
    /// <param name="settings">The settings to update</param>
    Task<bool> UpdateSettingsAsync(SystemSettings settings);
    
    /// <summary>
    /// Updates a specific setting
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    Task<bool> UpdateSettingAsync(string key, string value);
} 