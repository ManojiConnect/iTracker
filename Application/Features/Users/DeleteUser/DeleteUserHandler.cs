using Ardalis.Result;
using Application.Features.Common.Responses;
using Application.Abstractions.Data;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Infrastructure.Identity;

namespace Application.Features.Users.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly UserManager<Infrastructure.Identity.ApplicationUser> _userManager;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        IContext context, 
        UserManager<Infrastructure.Identity.ApplicationUser> userManager,
        ILogger<DeleteUserHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (user is null) return Result.NotFound();


        var identityUser = await _userManager.FindByIdAsync(user.UserId);

        //changing status of user IsActive False
        user.IsActive = false;

        //Delete IdentityUser
        if (identityUser != null)
        {
            await _userManager.DeleteAsync(identityUser!);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}