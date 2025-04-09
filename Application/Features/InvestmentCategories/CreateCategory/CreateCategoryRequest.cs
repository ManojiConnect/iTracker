using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Error("Name is required");
        }

        if (request.Name.Length > 100)
        {
            return Result.Error("Name cannot exceed 100 characters");
        }

        var existingCategory = await _context.InvestmentCategories
            .FirstOrDefaultAsync(c => c.Name == request.Name, cancellationToken);

        if (existingCategory != null)
        {
            return Result.Error("Category with this name already exists");
        }

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
        var result = await _context.SaveChangesAsync(cancellationToken);

        if (result <= 0)
        {
            return Result.Error("Failed to create category");
        }

        return Result.Success(category.Id);
    }
} 