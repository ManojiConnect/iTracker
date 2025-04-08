using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.InvestmentCategories.GetAllCategories;

public record GetAllCategoriesRequest : IRequest<Result<IEnumerable<CategoryDto>>>;

public record CategoryDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedOn { get; init; }
}

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesRequest, Result<IEnumerable<CategoryDto>>>
{
    private readonly IContext _context;

    public GetAllCategoriesHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
    {
        var categories = await _context.InvestmentCategories
            .Where(c => !c.IsDelete)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedOn = c.CreatedOn
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CategoryDto>>(categories);
    }
} 