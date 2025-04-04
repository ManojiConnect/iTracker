using Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, bool>
{
    private readonly IUserManager _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserManager userManager, 
        RoleManager<IdentityRole> roleManager, 
        ILogger<CreateUserHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with email: {Email}", request.Email);
        
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                _logger.LogWarning("User with email '{Email}' already exists", request.Email);
                throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
            }

            _logger.LogInformation("User not found, proceeding with creation");
            
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                Language = request.Language ?? "en",
                ProfileUrl = request.ProfileUrl,
                IsActive = request.IsActive
            };

            _logger.LogInformation("Calling CreateAsync for user {Email}", request.Email);
            var result = await _userManager.CreateAsync(newUser, request.Password);
            
            _logger.LogInformation("CreateAsync result: {Result}", result);
            
            if (result && !string.IsNullOrEmpty(request.Role))
            {
                _logger.LogInformation("Assigning role {Role} to user", request.Role);
                
                // Ensure the role exists
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    _logger.LogInformation("Role {Role} doesn't exist, creating it", request.Role);
                    await _roleManager.CreateAsync(new IdentityRole(request.Role));
                }
                
                try 
                {
                    // Fetch the newly created user
                    var createdUser = await _userManager.FindByEmailAsync(request.Email);
                    if (createdUser == null) 
                    {
                        _logger.LogError("Failed to find newly created user {Email}", request.Email);
                        return false;
                    }
                    
                    // Add user to role
                    _logger.LogInformation("Adding user {Email} to role {Role}", request.Email, request.Role);
                    var roleResult = await _userManager.AddToRoleAsync(createdUser, request.Role);
                    _logger.LogInformation("AddToRoleAsync result: {Result}", roleResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning role to user {Email}", request.Email);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            throw;
        }
    }
}
