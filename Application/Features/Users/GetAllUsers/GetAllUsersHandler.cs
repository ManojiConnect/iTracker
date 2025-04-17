using Ardalis.Result;
using Application.Features.Common.Responses;
using Application.Abstractions.Data;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Identity;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using ApplicationUser = Infrastructure.Identity.ApplicationUser;

namespace Application.Features.Users.GetAllUsers;
public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, Result<PaginatedList<UserResponse>>>
{
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(
        IContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<GetAllUsersHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<UserResponse>>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all users");
            
            // Get all users from AspNetUsers
            var identityUsers = await _userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} identity users", identityUsers.Count);
            
            // Get all existing User records in a single query
            var existingUsers = await _context.Users
                .AsNoTracking()
                .Where(u => !string.IsNullOrEmpty(u.UserId))
                .ToListAsync(cancellationToken);
                
            // Create dictionary for faster lookups
            var userDict = existingUsers.ToDictionary(u => u.UserId, u => u);
            
            // Keep track of new users that need to be added to Users table
            var newUserEntities = new List<User>();
            
            var userResponses = new List<UserResponse>();
            
            foreach (var identityUser in identityUsers)
            {
                try
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    
                    // Try to find matching User record
                    User userRecord = null;
                    bool existingUser = false;
                    
                    if (!string.IsNullOrEmpty(identityUser.Id))
                    {
                        existingUser = userDict.TryGetValue(identityUser.Id, out userRecord);
                    }
                    
                    // If no matching user record exists, create one
                    if (!existingUser)
                    {
                        _logger.LogInformation("Creating new User record for identity user {Email}", identityUser.Email);
                        
                        userRecord = new User
                        {
                            UserId = identityUser.Id,
                            FirstName = identityUser.FirstName,
                            LastName = identityUser.LastName,
                            Email = identityUser.Email,
                            PhoneNumber = identityUser.PhoneNumber,
                            Language = identityUser.Language,
                            ProfileUrl = identityUser.ProfileUrl,
                            IsActive = identityUser.IsActive,
                            IsDelete = false,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow
                        };
                        
                        newUserEntities.Add(userRecord);
                    }
                    
                    var userResponse = new UserResponse
                    {
                        Id = existingUser ? userRecord.Id : 0, // Temporary ID until saved
                        FirstName = identityUser.FirstName,
                        LastName = identityUser.LastName,
                        Email = identityUser.Email,
                        PhoneNumber = identityUser.PhoneNumber,
                        Language = identityUser.Language,
                        ProfileUrl = identityUser.ProfileUrl,
                        IsActive = existingUser ? userRecord.IsActive : identityUser.IsActive,
                        Role = roles.FirstOrDefault() ?? "No Role",
                        TransactionId = existingUser ? userRecord.Id : 0,
                        IdentityId = identityUser.Id
                    };
                    
                    userResponses.Add(userResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing user {UserId}", identityUser.Id);
                }
            }
            
            // Save any new user records
            if (newUserEntities.Count > 0)
            {
                try
                {
                    _logger.LogInformation("Adding {Count} new user records to Users table", newUserEntities.Count);
                    _context.Users.AddRange(newUserEntities);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    // Update IDs in the user response objects for the newly created records
                    // Re-fetch all users to get their assigned IDs
                    var updatedUsers = await _context.Users
                        .Where(u => newUserEntities.Select(nu => nu.UserId).Contains(u.UserId))
                        .ToListAsync(cancellationToken);
                        
                    var updatedDict = updatedUsers.ToDictionary(u => u.UserId, u => u);
                    
                    // Update user response objects with the newly created user IDs
                    foreach (var userResponse in userResponses)
                    {
                        if (userResponse.Id == 0 && updatedDict.TryGetValue(userResponse.IdentityId, out var updatedUser))
                        {
                            userResponse.Id = updatedUser.Id;
                            userResponse.TransactionId = updatedUser.Id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving new user records: {Message}", ex.Message);
                }
            }
            
            // Order users by name
            userResponses = userResponses.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
            
            var paginatedList = await userResponses.ToPaginatedListAsync<UserResponse, UserResponse>(
                request.CurrentPage, 
                request.PageSize, 
                request.Paging);
                
            return Result<PaginatedList<UserResponse>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return Result<PaginatedList<UserResponse>>.Error(ex.Message);
        }
    }
}
