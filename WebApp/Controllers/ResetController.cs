using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResetController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("admin-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetAdminPassword()
    {
        try
        {
            var adminEmail = "Admin@itrackerApp.com";
            var newPassword = "Test@123";

            var user = await _userManager.FindByEmailAsync(adminEmail);
            
            if (user == null)
            {
                return NotFound($"User with email {adminEmail} not found.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                return Ok($"Password for {adminEmail} has been reset to {newPassword}");
            }
            else
            {
                return BadRequest($"Failed to reset password: {string.Join(", ", result.Errors)}");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}. Stack trace: {ex.StackTrace}");
        }
    }
} 