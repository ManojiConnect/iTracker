using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;

namespace Application.Features.InvestmentCategories.CreateCategory;

public record CreateCategoryRequest : IRequest<Result<int>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class CreateCategoryHandler : IRequestHandler<CreateCategoryRequest, Result<int>>
{
    private readonly IContext _context;

    public CreateCategoryHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Domain.Entities.InvestmentCategory
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = 1,
            ModifiedOn = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false
        };

        _context.InvestmentCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
} 