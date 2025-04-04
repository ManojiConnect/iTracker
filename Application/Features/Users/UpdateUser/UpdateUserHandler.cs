using Ardalis.Result;
using Infrastructure.Common;
using Infrastructure.Context;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Application.Features.Users.UpdateUser;
public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        IContext context, 
        ICurrentUserService currentUserService, 
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Try to find existing user by ID
            var existingUser = await _context.Users.FirstOrDefaultAsync(
                x => x.Id == request.Id && x.IsDelete == false, 
                cancellationToken);
            
            // If we can't find by ID but have an IdentityId, try to find or create a user record
            if (existingUser == null && !string.IsNullOrEmpty(request.IdentityId))
            {
                _logger.LogInformation("User with ID {Id} not found, looking up by IdentityId {IdentityId}", 
                    request.Id, request.IdentityId);
                
                // Check if there's already a user record with this IdentityId
                existingUser = await _context.Users.FirstOrDefaultAsync(
                    x => x.UserId == request.IdentityId && x.IsDelete == false,
                    cancellationToken);
                
                // If still no user found, create a new one linked to the identity user
                if (existingUser == null)
                {
                    _logger.LogInformation("Creating new User record for identity user {IdentityId}", request.IdentityId);
                    
                    existingUser = new User
                    {
                        UserId = request.IdentityId,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        Language = request.Language,
                        ProfileUrl = request.ProfileUrl,
                        IsActive = request.IsActive,
                        IsDelete = false,
                        CreatedBy = 1, // Default to admin
                        CreatedOn = DateTime.UtcNow
                    };
                    
                    _context.Users.Add(existingUser);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Created new User record with ID {Id} for identity user {IdentityId}", 
                        existingUser.Id, request.IdentityId);
                }
            }
            
            if (existingUser == null)
            {
                _logger.LogWarning("User not found with ID {Id} or IdentityId {IdentityId}", 
                    request.Id, request.IdentityId);
                return Result.NotFound("User not found");
            }
            
            // Update the user record
            existingUser = request.Adapt(existingUser);
            if (string.IsNullOrEmpty(existingUser.Language))
            {
                existingUser.Language = _configuration.GetSection("Language").GetValue<string>("Default");
            }
            
            // If we have an identity ID, make sure it's preserved
            if (!string.IsNullOrEmpty(request.IdentityId))
            {
                existingUser.UserId = request.IdentityId;
            }
            
            _context.Users.Update(existingUser);
            
            // If we have an identity ID, also update the application user
            if (!string.IsNullOrEmpty(request.IdentityId))
            {
                var identityUser = await _userManager.FindByIdAsync(request.IdentityId);
                if (identityUser != null)
                {
                    identityUser.FirstName = request.FirstName;
                    identityUser.LastName = request.LastName;
                    identityUser.Email = request.Email;
                    identityUser.PhoneNumber = request.PhoneNumber;
                    identityUser.Language = request.Language;
                    identityUser.ProfileUrl = request.ProfileUrl;
                    identityUser.IsActive = request.IsActive;
                    
                    var identityResult = await _userManager.UpdateAsync(identityUser);
                    if (!identityResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update identity user: {Errors}", 
                            string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", request.Id);
            return Result.Error($"Error updating user: {ex.Message}");
        }
    }
}