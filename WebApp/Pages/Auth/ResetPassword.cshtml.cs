using Application.Features.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Identity;

namespace WebApp.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;

    [BindProperty]
    public ResetPasswordRequest ResetPasswordRequest { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string Email { get; set; }
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; }
    public ResetPasswordModel(IMediator mediator, UserManager<Infrastructure.Identity.ApplicationUser> userManager)
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
            UserManager<Infrastructure.Identity.ApplicationUser>.ResetPasswordTokenPurpose, Token);
        if (!isTokenValid)
        {
            return RedirectToPage("/Common/DisplayMessage", new { message = "The password reset link has expired." });
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(ResetPasswordRequest);
        if (result.IsSuccess)
        {
            return RedirectToPage("/Auth/Login", new { message = "Your password has been reset successfully. Please login with your new password." });
        }

        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Failed to reset password. Please try again.");
        return Page();
    }
}