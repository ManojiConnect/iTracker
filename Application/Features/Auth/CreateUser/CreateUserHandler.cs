using Domain.Interfaces;
using MediatR;
using Infrastructure.Identity;

namespace Application.Features.Auth.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, bool>
{
    private readonly IUserManager _userManager;

    public CreateUserHandler(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }

        var newUser = new Infrastructure.Identity.ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true,
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        return await _userManager.CreateAsync(newUser, request.Password);
    }
} 