using Ardalis.Result;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Application.Features.Users.CreateUser;
using Domain.Interfaces;
using Application.Abstractions.Services;
using Infrastructure.Identity;

namespace Application.Services;
public class UserService
{
    private readonly IContext _context;
    private readonly IMediator _mediator;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMailService _mailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserManager _userManagerService;

    public UserService(
        IContext context, 
        IMediator mediator, 
        UserManager<Infrastructure.Identity.ApplicationUser> userManager, 
        IConfiguration configuration, 
        IMailService mailService,
        ICurrentUserService currentUserService,
        IUserManager userManagerService)
    {
        _context = context;
        _mediator = mediator;
        _userManager = userManager;
        _configuration = configuration;
        _mailService = mailService;
        _currentUserService = currentUserService;
        _userManagerService = userManagerService;
    }

    public async Task<Result<User>> CreateNewUserAsync(string email, string firstName, string lastName, string phoneNumber, CancellationToken cancellationToken, bool sendMail = true)
    {
        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var user = await CreateNewUserAsyncTransaction(_context, email, firstName, lastName, phoneNumber, cancellationToken);
                scope.Complete();
                return user;
            }
            catch
            {
                throw;
            }
        }
    }

    public async Task<Result<User>> CreateNewUserAsyncTransaction(IContext context, string email, string firstName, string lastName, string phoneNumber, CancellationToken cancellationToken, bool sendMail = true)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result.Error("User with provided email id already exists.");
        }

        var identityUser = new Infrastructure.Identity.ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(identityUser);
        if (!result.Succeeded)
        {
            return Result.Error("Failed to create user account.");
        }

        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            Language = _configuration.GetSection("Language").GetValue<string>("Default"),
            UserId = identityUser.Id,
            CreatedBy = 1, // Default to admin user (ID: 1) for initial setup
            ModifiedBy = 1 // Default to admin user (ID: 1) for initial setup
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        if (sendMail)
        {
            // Send welcome email
            var subject = "Welcome to iTracker";
            var plainTextContent = $"Welcome {firstName}! Your account has been created successfully.";
            var htmlContent = $"<h1>Welcome {firstName}!</h1><p>Your account has been created successfully.</p>";
            await _mailService.SendEmailAsync(email, subject, plainTextContent, htmlContent);
        }

        return Result.Success(user);
    }

    public async Task<bool> ConfirmEmail(string email)
    {
        try
        {
            var user = await _userManagerService.FindByEmailAsync(email);
            if (user == null)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IApplicationUser?> GetUserByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }

        return await _userManagerService.FindByEmailAsync(email);
    }
}
