using Ardalis.Result;
using Application.Features.Common.Responses;
using Application.Abstractions.Data;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Infrastructure.Identity;

namespace Application.Features.Users.GetUserById;
public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, Result<UserResponse>>
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly ILogger<GetUserByIdHandler> _logger;

    public GetUserByIdHandler(
        IContext context, 
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        ILogger<GetUserByIdHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Safety check - don't allow ID 0
            if (request.Id == 0)
            {
                _logger.LogWarning("Attempted to get user with invalid ID 0");
                return Result.NotFound("User not found");
            }
            
            _logger.LogInformation("Fetching user with ID: {Id}", request.Id);
            
            var result = await _context.Users.AsNoTracking()
                 .Where(u => u.IsActive == true && u.IsDelete == false)
                 .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken: cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("User with ID {Id} not found", request.Id);
                return Result.NotFound("User not found");
            }

            // Safety check on user ID
            if (string.IsNullOrEmpty(result.UserId))
            {
                _logger.LogWarning("User with ID {Id} has no identity user reference", request.Id);
                return Result.NotFound("User identity not found");
            }

            var identityUser = await _userManager.FindByIdAsync(result.UserId);
            if (identityUser == null)
            {
                _logger.LogWarning("Identity user not found for user ID {Id}", request.Id);
                return Result.NotFound("User identity not found");
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var userResponse = result.Adapt<UserResponse>();
            userResponse.Role = roles.FirstOrDefault() ?? "No Role";
            userResponse.TransactionId = userResponse.Id;
            
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {Id}", request.Id);
            return Result.Error($"Error retrieving user: {ex.Message}");
        }
    }
}
