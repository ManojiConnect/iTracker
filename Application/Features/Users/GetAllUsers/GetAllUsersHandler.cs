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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.GetAllUsers;
public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, Result<PaginatedList<UserResponse>>>
{
    private readonly IContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersHandler(IContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<PaginatedList<UserResponse>>> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        List<string> roleStr = new List<string>();
        roleStr.Add("Admin");
        roleStr.Add("Organizer");
        var result = await _context.Users.AsNoTracking()
        .Where(u => u.IsActive == true)
        .OrderByDescending(u => u.CreatedOn)
        .ToListAsync(cancellationToken: cancellationToken);

        var userResponse = result.Adapt<List<UserResponse>>();
        foreach (var userResponse1 in userResponse)
        {
            var user = await _userManager.FindByIdAsync((await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userResponse1.Id, cancellationToken))!.UserId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user!);
                userResponse1.Role = roles[0];
            }
        }
        var resultlst = await (userResponse.Where(a => roleStr.Any(x => x == a.Role))).ToPaginatedListAsync<UserResponse, UserResponse>(request.CurrentPage, request.PageSize, request.Paging);
        return Result<PaginatedList<UserResponse>>.Success(resultlst);
    }
}
