using System;
using System.Threading.Tasks;
using Application.Features.Common.Responses;
using Application.Features.Users.DeleteUser;
using Application.Features.Users.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.Users;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteModel> _logger;

    [BindProperty]
    public UserResponse User { get; set; } = new();

    public DeleteModel(IMediator mediator, ILogger<DeleteModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving user with ID: {Id} for deletion", id);
            
            var result = await _mediator.Send(new GetUserByIdRequest(id));
            
            if (!result.IsSuccess || result.Value == null)
            {
                _logger.LogWarning("User with ID {Id} not found", id);
                return NotFound();
            }

            User = result.Value;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {Id}", id);
            return StatusCode(500);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {Id}", User.Id);
            
            var result = await _mediator.Send(new DeleteUserRequest(User.Id));
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("User with ID {Id} deleted successfully", User.Id);
                TempData["SuccessMessage"] = "User was deleted successfully.";
                return RedirectToPage("./Index");
            }
            else
            {
                _logger.LogWarning("Failed to delete user with ID {Id}", User.Id);
                ModelState.AddModelError(string.Empty, "Failed to delete user. Please try again.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {Id}", User.Id);
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
} 