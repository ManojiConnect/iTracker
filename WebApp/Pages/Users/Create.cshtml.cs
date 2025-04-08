using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Application.Features.Users.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.Users;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateModel> _logger;

    [BindProperty]
    public UserFormModel UserForm { get; set; } = new();

    public CreateModel(IMediator mediator, ILogger<CreateModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public void OnGet()
    {
        // Default values
        UserForm.Language = "en";
        UserForm.IsActive = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Creating new user: {Email}", UserForm.Email);

            var request = new CreateUserRequest
            {
                FirstName = UserForm.FirstName!,
                LastName = UserForm.LastName!,
                Email = UserForm.Email!,
                Password = UserForm.Password!,
                PhoneNumber = UserForm.PhoneNumber,
                Role = UserForm.Role!,
                Language = UserForm.Language,
                ProfileUrl = UserForm.ProfileUrl,
                IsActive = UserForm.IsActive
            };

            var result = await _mediator.Send(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User created successfully: {Email}", UserForm.Email);
                TempData["SuccessMessage"] = $"User {UserForm.FirstName} {UserForm.LastName} was created successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                _logger.LogWarning("Failed to create user: {Email}", UserForm.Email);
                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Failed to create user. Please try again.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", UserForm.Email);
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }

    public class UserFormModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string? Role { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Profile URL")]
        public string? ProfileUrl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
} 