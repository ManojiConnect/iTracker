using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using Domain.Entities;

namespace WebApp.Controllers;

public class ResetPasswordController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetAdminPassword()
    {
        try
        {
            string adminEmail = "Admin@itrackerApp.com";
            string newPassword = "Test@123";

            var user = await _userManager.FindByEmailAsync(adminEmail);
            
            if (user == null)
            {
                ViewBag.Error = $"Admin user with email {adminEmail} not found.";
                return View("Index");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                ViewBag.Success = $"Admin password has been reset to '{newPassword}'";
                return View("Index");
            }
            
            ViewBag.Error = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"An error occurred: {ex.Message}\n{ex.StackTrace}";
            return View("Index");
        }
    }
} 