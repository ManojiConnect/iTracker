using Application.Features.Auth.Authenticate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IMediator mediator, ILogger<LoginModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [BindProperty]
    public bool RememberMe { get; set; }

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

        _logger.LogInformation("Attempting login for user: {Email}", Email);

        var request = new AuthenticateRequest
        {
            Email = Email,
            Password = Password,
            RememberMe = RememberMe
        };

        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Login successful for user: {Email}", Email);
            _logger.LogInformation("Attempting to redirect to Portfolios/Index");
            return RedirectToPage("/Portfolios/Index");
        }

        if (result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            ErrorMessage = result.ValidationErrors.First().ErrorMessage;
            _logger.LogWarning("Login failed for user {Email}: {ErrorMessage}", Email, ErrorMessage);
        }
        else
        {
            ErrorMessage = "Invalid email or password.";
            _logger.LogWarning("Login failed for user {Email}: Invalid credentials", Email);
        }

        return Page();
    }
}