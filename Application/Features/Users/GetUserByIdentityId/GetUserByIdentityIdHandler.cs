using Ardalis.Result;
using Application.Features.Common.Responses;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Identity;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Application.Features.Users.GetUserByIdentityId;
public class GetUserByIdentityIdHandler : IRequestHandler<GetUserByIdentityIdRequest, Result<UserResponse>>
{
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetUserByIdentityIdHandler> _logger;

    public GetUserByIdentityIdHandler(
        IContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<GetUserByIdentityIdHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdentityIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Safety check - don't allow null or empty identity ID
            if (string.IsNullOrEmpty(request.IdentityId))
            {
                _logger.LogWarning("Attempted to get user with invalid identity ID");
                return Result.NotFound("User not found");
            }
            
            _logger.LogInformation("Fetching user with identity ID: {IdentityId}", request.IdentityId);
            
            // First, get the identity user
            var identityUser = await _userManager.FindByIdAsync(request.IdentityId);
            if (identityUser == null)
            {
                _logger.LogWarning("Identity user with ID {IdentityId} not found", request.IdentityId);
                return Result.NotFound("User not found");
            }
            
            // Get roles for this user
            var roles = await _userManager.GetRolesAsync(identityUser);
            
            // Try to find matching entry in Users table
            var userRecord = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == request.IdentityId, cancellationToken);
            
            // Create user response from identity user
            var userResponse = new UserResponse
            {
                Id = userRecord?.Id ?? 0, // Use 0 if no matching record
                FirstName = identityUser.FirstName,
                LastName = identityUser.LastName,
                Email = identityUser.Email,
                PhoneNumber = identityUser.PhoneNumber,
                Language = identityUser.Language,
                ProfileUrl = identityUser.ProfileUrl,
                IsActive = identityUser.IsActive,
                Role = roles.FirstOrDefault() ?? "No Role",
                TransactionId = userRecord?.Id ?? 0,
                IdentityId = identityUser.Id
            };
            
            return Result<UserResponse>.Success(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with identity ID: {IdentityId}", request.IdentityId);
            return Result.Error($"Error retrieving user: {ex.Message}");
        }
    }
} 