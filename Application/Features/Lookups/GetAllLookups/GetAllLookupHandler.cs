using Ardalis.Result;
using Application.Extensions;
using Application.Features.Common.Responses;
using Application.Features.Lookups.Responses;
using Infrastructure.Context;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;

namespace Application.Features.Lookups.GetAllLookups;

public class GetAllLookupHandler : IRequestHandler<GetAllLookupRequest, Result<PaginatedList<LookupResponse>>>
{
    private readonly IContext _context;

    public GetAllLookupHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<LookupResponse>>> Handle(GetAllLookupRequest request, CancellationToken cancellationToken)
    {
        var query = _context.Lookups
            .Include(x => x.Type)
            .Where(x => x.Type.Name == request.Type)
            .Select(x => new LookupResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Type = x.Type.Name
            });

        var paginatedList = await PaginatedList<LookupResponse>.CreateAsync(query, request.PageNumber, request.PageSize);

        return Result.Success(paginatedList);
    }
}
