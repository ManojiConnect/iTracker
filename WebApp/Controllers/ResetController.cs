using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Identity;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResetController : ControllerBase
{
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ResetController(
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
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
            var adminRole = "Admin";

            // Check if admin role exists
            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Check if admin user exists
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // Create admin user
                adminUser = new Infrastructure.Identity.ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    return BadRequest($"Failed to create admin user: {string.Join(", ", result.Errors)}");
                }

                // Add admin role
                result = await _userManager.AddToRoleAsync(adminUser, adminRole);
                if (!result.Succeeded)
                {
                    return BadRequest($"Failed to add admin role: {string.Join(", ", result.Errors)}");
                }
            }

            return Ok("Admin user and role have been ensured.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}. Stack trace: {ex.StackTrace}");
        }
    }
} 