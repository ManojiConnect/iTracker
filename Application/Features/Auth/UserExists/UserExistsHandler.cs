using Ardalis.Result;
using Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Features.Auth.UserExists;
public class UserExistsHandler : IRequestHandler<UserExistsRequest, Result<bool>>
{
    private readonly IContext _context;

    public UserExistsHandler(IContext context)
    {
        _context = context;

    }

    public async Task<Result<bool>> Handle(UserExistsRequest request, CancellationToken cancellationToken)
    {
        //find user by email
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user is null) return Result<bool>.Success(false);
        return Result<bool>.Success(true);
    }
}
