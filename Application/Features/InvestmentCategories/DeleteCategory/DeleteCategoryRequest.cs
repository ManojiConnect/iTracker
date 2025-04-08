using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.InvestmentCategories.DeleteCategory;

public record DeleteCategoryRequest : IRequest<Result<int>>
{
    public int Id { get; init; }
}

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryRequest, Result<int>>
{
    private readonly IContext _context;

    public DeleteCategoryHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.InvestmentCategories.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (category == null)
        {
            return Result.NotFound("Category not found");
        }

        // Check if category has any investments
        var hasInvestments = await _context.Investments.AnyAsync(i => i.CategoryId == request.Id && !i.IsDelete, cancellationToken);
        if (hasInvestments)
        {
            return Result.Error("Cannot delete category with existing investments");
        }

        category.IsDelete = true;
        category.ModifiedBy = 1;
        category.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }
} 