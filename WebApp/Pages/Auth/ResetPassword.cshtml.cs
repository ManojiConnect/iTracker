using Application.Features.Auth.ResetPassword;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Flurl.Util;

namespace WebApp.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<IdentityUser> _userManager;

    [BindProperty]
    public ResetPasswordRequest ResetPasswordRequest { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string Email { get; set; }
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; }
    public ResetPasswordModel(IMediator mediator, UserManager<IdentityUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var user = await _userManager.FindByEmailAsync(Email);
        var isTokenValid = await _userManager.VerifyUserTokenAsync(user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<IdentityUser>.ResetPasswordTokenPurpose, Token);
        if (!isTokenValid)
        {
            return RedirectToPage("/Common/DisplayMessage", new { message = "The password reset link has expired." });
        }
        return Page();

    }

    public async Task<IActionResult> OnPostAsync()
    {
        ResetPasswordRequest = new ResetPasswordRequest
        {
            Email = Email,
            Token = Token,
            ConfirmPassword = ResetPasswordRequest.ConfirmPassword,
            NewPassword = ResetPasswordRequest.NewPassword
        };
        var result = await _mediator.Send(ResetPasswordRequest);

        if (result.IsSuccess)
        {
            return RedirectToPage("/Common/DisplayMessage", new { message = "Your password has been reset successfully" });
        }
        foreach (var error in result.ValidationErrors)
        {
            // Get the property name from the Identifier
            string propertyName = error.Identifier ?? string.Empty;

                // Add the error for the specific property
               ModelState.AddModelError($"ResetPasswordRequest.{propertyName}", error.ErrorMessage);
            
        }
        return Page();
    }
}