using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Infrastructure.Identity;
using Domain.Entities;

namespace WebApp.Pages.Auth;

public class ResetAdminPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResetAdminPasswordModel> _logger;

    public ResetAdminPasswordModel(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<ResetAdminPasswordModel> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Admin email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string AdminEmail { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Security key is required")]
    public string SecurityKey { get; set; }

    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verify security key
        var expectedSecurityKey = _configuration["AdminResetKey"] ?? "iTrackerAdminReset2025";
        if (SecurityKey != expectedSecurityKey)
        {
            ModelState.AddModelError(string.Empty, "Invalid security key");
            ErrorMessage = "The security key you provided is incorrect.";
            return Page();
        }

        try
        {
            var user = await _userManager.FindByEmailAsync(AdminEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                ErrorMessage = "No user found with the provided email address.";
                return Page();
            }

            // Check if user is in Admin role
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                ModelState.AddModelError(string.Empty, "User is not an admin");
                ErrorMessage = "The provided email does not belong to an admin user.";
                return Page();
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin password reset successful for user: {Email}", AdminEmail);
                SuccessMessage = "Admin password has been reset successfully. You can now login with the new password.";
                
                // Clear form
                AdminEmail = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                SecurityKey = string.Empty;
                
                return Page();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                ErrorMessage = "Failed to reset password. Please check the errors above.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting admin password for {Email}", AdminEmail);
            ErrorMessage = "An error occurred while resetting the password. Please try again later.";
            return Page();
        }
    }
} 