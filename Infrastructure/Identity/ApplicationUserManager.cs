using Domain.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

public class ApplicationUserManager : UserManager<ApplicationUser>, IUserManager
{
    public ApplicationUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public async Task<bool> CreateAsync(IApplicationUser user, string password)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        var result = await base.CreateAsync(applicationUser, password);
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(IApplicationUser user, string token, string newPassword)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        var result = await base.ResetPasswordAsync(applicationUser, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(IApplicationUser user, string currentPassword, string newPassword)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        var result = await base.ChangePasswordAsync(applicationUser, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> CheckPasswordAsync(IApplicationUser user, string password)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        return await base.CheckPasswordAsync(applicationUser, password);
    }

    public async Task<bool> IsEmailConfirmedAsync(IApplicationUser user)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        return await base.IsEmailConfirmedAsync(applicationUser);
    }

    public new async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        return await base.FindByIdAsync(userId);
    }

    async Task<IApplicationUser> IUserManager.FindByIdAsync(string userId)
    {
        var user = await FindByIdAsync(userId);
        return user ?? throw new InvalidOperationException($"User with ID '{userId}' not found.");
    }

    public new async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return await base.FindByEmailAsync(email);
    }

    async Task<IApplicationUser> IUserManager.FindByEmailAsync(string email)
    {
        var user = await FindByEmailAsync(email);
        return user ?? throw new InvalidOperationException($"User with email '{email}' not found.");
    }

    public async Task<string> GeneratePasswordResetTokenAsync(IApplicationUser user)
    {
        if (user is not ApplicationUser applicationUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        return await base.GeneratePasswordResetTokenAsync(applicationUser);
    }
} 