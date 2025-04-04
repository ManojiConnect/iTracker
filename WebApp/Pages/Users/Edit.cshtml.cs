using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Application.Features.Common.Responses;
using Application.Features.Users.GetUserById;
using Application.Features.Users.GetUserByIdentityId;
using Application.Features.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.Users;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<EditModel> _logger;

    [BindProperty]
    public UserFormModel UserForm { get; set; } = new();
    
    public string CurrentRole { get; set; } = string.Empty;
    
    [BindProperty]
    public string? IdentityId { get; set; }

    public EditModel(IMediator mediator, ILogger<EditModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int? id, string? identityId)
    {
        try
        {
            if (id.HasValue && id.Value > 0)
            {
                _logger.LogInformation("Retrieving user with ID: {Id}", id);
                
                var result = await _mediator.Send(new GetUserByIdRequest(id.Value));
                
                if (!result.IsSuccess || result.Value == null)
                {
                    _logger.LogWarning("User with ID {Id} not found", id);
                    return NotFound();
                }
                
                var user = result.Value;
                IdentityId = user.IdentityId;
                CurrentRole = user.Role ?? "No Role";
                
                UserForm = new UserFormModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Language = user.Language ?? "en",
                    ProfileUrl = user.ProfileUrl,
                    IsActive = user.IsActive ?? true
                };
            }
            else if (!string.IsNullOrEmpty(identityId))
            {
                _logger.LogInformation("Retrieving user with IdentityId: {IdentityId}", identityId);
                
                var result = await _mediator.Send(new GetUserByIdentityIdRequest(identityId));
                
                if (!result.IsSuccess || result.Value == null)
                {
                    _logger.LogWarning("User with IdentityId {IdentityId} not found", identityId);
                    return NotFound();
                }
                
                var user = result.Value;
                IdentityId = user.IdentityId;
                CurrentRole = user.Role ?? "No Role";
                
                UserForm = new UserFormModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    Language = user.Language ?? "en",
                    ProfileUrl = user.ProfileUrl,
                    IsActive = user.IsActive ?? true
                };
            }
            else
            {
                _logger.LogWarning("No ID or IdentityId provided");
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Updating user: {Id}", UserForm.Id);

            var request = new UpdateUserRequest
            {
                Id = UserForm.Id,
                FirstName = UserForm.FirstName,
                LastName = UserForm.LastName,
                Email = UserForm.Email,
                PhoneNumber = UserForm.PhoneNumber,
                Language = UserForm.Language,
                ProfileUrl = UserForm.ProfileUrl,
                IsActive = UserForm.IsActive,
                IdentityId = IdentityId // Pass through the identity ID
            };

            var result = await _mediator.Send(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User updated successfully: {Id}", UserForm.Id);
                TempData["SuccessMessage"] = "User was updated successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                _logger.LogWarning("Failed to update user: {Id}", UserForm.Id);
                ModelState.AddModelError(string.Empty, "Failed to update user. Please try again.");
                return Page();
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, "The user has been modified by someone else. Please refresh and try again.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", UserForm.Id);
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }

    public class UserFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string? Role { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; } = "en";

        [Display(Name = "Profile Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ProfileUrl { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
} 