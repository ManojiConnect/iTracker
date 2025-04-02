using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Pages;

public class AdminToolsModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    [TempData]
    public string SuccessMessage { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    [BindProperty]
    public string Email { get; set; } = "Admin@itrackerApp.com";

    [BindProperty]
    public string Password { get; set; } = "Test@123";

    public AdminToolsModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(Email);
            
            if (user == null)
            {
                ErrorMessage = $"User with email {Email} not found.";
                return Page();
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, token, Password);
            
            if (result.Succeeded)
            {
                SuccessMessage = "Password has been reset successfully.";
            }
            else
            {
                ErrorMessage = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            return Page();
        }
    }
} 