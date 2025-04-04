using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Common.Responses;
using Application.Features.Users.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace WebApp.Pages.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexModel> _logger;

    public List<UserResponse> Users { get; set; } = new List<UserResponse>();

    public IndexModel(IMediator mediator, ILogger<IndexModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all users");
            
            var request = new GetAllUsersRequest
            {
                CurrentPage = 1,
                PageSize = 100,
                Paging = false
            };
            
            var result = await _mediator.Send(request);

            if (result.IsSuccess)
            {
                Users = result.Value.Items;
                _logger.LogInformation("Successfully retrieved {Count} users", Users.Count);
                
                // Temporary: Check for specific user
                var specificEmail = "rahul@iconnectsolutions.com";
                var hasUser = Users.Any(u => u.Email?.Equals(specificEmail, StringComparison.OrdinalIgnoreCase) == true);
                _logger.LogInformation("User with email {Email} exists: {Exists}", specificEmail, hasUser);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve users: {Message}", result.Errors);
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500);
        }
    }
} 