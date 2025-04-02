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

namespace Application.Features.Users.GetUserById;
public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, Result<UserResponse>>
{
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdHandler(IContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
    {
        var result = await _context.Users.AsNoTracking()
             .Where(u => u.IsActive == true && u.IsDelete == false)
             .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken: cancellationToken);

        if (result! == null)
        {
            // Return a 204 No Content response when the user is not found
            return null;
        }

        var identityUser = await _userManager.FindByIdAsync(_context.Users.First(u => u.Id == result.Id).UserId);
        var roles = await _userManager.GetRolesAsync(identityUser!);
        var userResponse = result.Adapt<UserResponse>();
        userResponse.Role = roles[0];
        /* if (userResponse.Role == "Driver")
            userResponse.TransactionId = await _context.Drivers.Where(a => a.UserId == request.Id).Select(a => a.Id).FirstOrDefaultAsync(cancellationToken);
        else if (userResponse.Role == "Contact")
            userResponse.TransactionId = await _context.Contacts.Where(a => a.UserId == request.Id).Select(a => a.Id).FirstOrDefaultAsync(cancellationToken);
        else if (userResponse.Role == "SchoolCoordinator")
            userResponse.TransactionId = await _context.SchoolUsers.Where(a => a.UserId == request.Id).Select(a => a.Id).FirstOrDefaultAsync(cancellationToken);
        else */
            userResponse.TransactionId = userResponse.Id;
        return Result<UserResponse>.Success(userResponse);
    }
}
