using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;

namespace Application.Features.InvestmentCategories.UpdateCategory;

public record UpdateCategoryRequest : IRequest<Result<int>>
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryRequest, Result<int>>
{
    private readonly IContext _context;

    public UpdateCategoryHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.InvestmentCategories.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (category == null)
        {
            return Result.NotFound("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ModifiedBy = 1;
        category.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }
} 