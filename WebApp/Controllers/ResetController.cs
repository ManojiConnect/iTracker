using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Domain.Entities;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResetController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ResetController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

    [HttpGet("ensure-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> EnsureAdminExists()
    {
        try
        {
            var adminEmail = "Admin@itrackerApp.com";
            var adminPassword = "Test@123";

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var user = await _userManager.FindByEmailAsync(adminEmail);
            
            if (user == null)
            {
                // Create new admin user
                user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                var createResult = await _userManager.CreateAsync(user, adminPassword);
                if (!createResult.Succeeded)
                {
                    return BadRequest($"Failed to create admin user: {string.Join(", ", createResult.Errors)}");
                }
            }
            else
            {
                // Reset password for existing user
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, adminPassword);
                if (!resetResult.Succeeded)
                {
                    return BadRequest($"Failed to reset password: {string.Join(", ", resetResult.Errors)}");
                }
            }

            // Ensure user is in Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!addToRoleResult.Succeeded)
                {
                    return BadRequest($"Failed to add user to Admin role: {string.Join(", ", addToRoleResult.Errors)}");
                }
            }

            return Ok($"Admin user ensured with email {adminEmail} and password {adminPassword}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}. Stack trace: {ex.StackTrace}");
        }
    }
} 