using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Auth.ForgotPassword;
namespace WebApp.Pages.Auth;
public class ForgotPasswordModel : PageModel
{
    private readonly IMediator _mediator;

    public ForgotPasswordModel(IMediator mediator)
    {
        _mediator = mediator;
    }
    [TempData]
    public string ErrorMessage { get; set; }

    [BindProperty]
    public ForgotPasswordRequest Input { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _mediator.Send(Input);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Common/DisplayMessage", new { message = "The password reset link is sent on mail" });
            }

            foreach (var error in result.ValidationErrors)
            {
                // Get the property name from the Identifier
                string propertyName = error.Identifier ?? string.Empty;

                // If the property name is empty, add it as a model-level error
                if (error.Identifier == "Incorrect")
                {
                    // Add email existence error
                    ModelState.AddModelError("Input.Email", error.ErrorMessage);
                }
                else
                {
                    // Add the error for the specific property
                    ModelState.AddModelError($"Input.{propertyName}", error.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
        }

        return Page();
    }
}