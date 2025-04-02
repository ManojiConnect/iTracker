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

namespace Application.Features.Users.UpdateUser;
public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, Result<bool>>
{
    private readonly IContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;

    public UpdateUserHandler(IContext context, ICurrentUserService currentUserService, IConfiguration configuration)
    {
        _context = context;
        _currentUserService = currentUserService;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var originalUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsDelete == false, cancellationToken);
        if (originalUser is null) return Result.NotFound();

        originalUser = request.Adapt(originalUser);
        if (string.IsNullOrEmpty(originalUser.Language))
        {
            originalUser.Language = _configuration.GetSection("Language").GetValue<string>("Default");
        }
        _context.Users.Update(originalUser);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}