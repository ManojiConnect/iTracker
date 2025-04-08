using Application.Abstractions.Data;
using Application.Common.Interfaces;
using Ardalis.Result;
using Infrastructure.Identity;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Abstractions.Services;

namespace Application.Features.Users.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly ILogger<CreateUserHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly IMailService _mailService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CreateUserHandler(
        IContext context,
        ILogger<CreateUserHandler> logger,
        IConfiguration configuration,
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        IMailService mailService,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _mailService = mailService;
        _roleManager = roleManager;
    }

    public async Task<Result<bool>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with email: {Email}", request.Email);
        
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email '{Email}' already exists", request.Email);
                return Result.Error($"User with email '{request.Email}' already exists.");
            }

            _logger.LogInformation("User not found, proceeding with creation");
            
            var newUser = new Infrastructure.Identity.ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName!,
                LastName = request.LastName!,
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
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created successfully");

                var createdUser = await _userManager.FindByEmailAsync(request.Email);
                if (createdUser == null)
                {
                    _logger.LogError("Failed to find newly created user {Email}", request.Email);
                    return Result.Error("Failed to find newly created user.");
                }
                
                try
                {
                    await _userManager.AddToRoleAsync(createdUser, request.Role!);
                    _logger.LogInformation("Role {Role} assigned to user {Email}", request.Role, request.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning role to user {Email}", request.Email);
                    return Result.Error("Error assigning role to user.");
                }
            }

            return Result.Success(result.Succeeded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            return Result.Error("Error creating user.");
        }
    }
}
