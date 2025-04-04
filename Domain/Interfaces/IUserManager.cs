using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserManager
{
    Task<IApplicationUser> FindByIdAsync(string userId);
    Task<IApplicationUser> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(IApplicationUser user, string password);
    Task<bool> IsEmailConfirmedAsync(IApplicationUser user);
    Task<string> GeneratePasswordResetTokenAsync(IApplicationUser user);
    Task<bool> ResetPasswordAsync(IApplicationUser user, string token, string newPassword);
    Task<bool> ChangePasswordAsync(IApplicationUser user, string currentPassword, string newPassword);
    Task<bool> CreateAsync(IApplicationUser user, string password);
    Task<bool> AddToRoleAsync(IApplicationUser user, string role);
} 