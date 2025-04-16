using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.InvestmentCategories.CreateCategory;

public record CreateCategoryRequest : IRequest<Result<int>>
{
    public required string Name { get; init; }
    public string? Description { get; init; } = string.Empty;
}

public class CreateCategoryHandler : IRequestHandler<CreateCategoryRequest, Result<int>>
{
    private readonly IContext _context;
    private readonly ILogger<CreateCategoryHandler> _logger;

    public CreateCategoryHandler(IContext context, ILogger<CreateCategoryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Category creation failed: Name is required");
                return Result.Error("Name is required");
            }

            if (request.Name.Length > 100)
            {
                _logger.LogWarning("Category creation failed: Name too long ({Length} chars)", request.Name.Length);
                return Result.Error("Name cannot exceed 100 characters");
            }

            var existingCategory = await _context.InvestmentCategories
                .FirstOrDefaultAsync(c => c.Name == request.Name && !c.IsDelete, cancellationToken);

            if (existingCategory != null)
            {
                _logger.LogWarning("Category creation failed: Active category with name '{Name}' already exists", request.Name);
                return Result.Error("A category with this name already exists");
            }

            // Ensure description is never null when saving to database
            string description = request.Description ?? string.Empty;
            _logger.LogInformation("Creating new category: {Name}, Description length: {Length}", 
                request.Name, description.Length);

            var category = new Domain.Entities.InvestmentCategory
            {
                Name = request.Name,
                Description = description,
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = 1,
                ModifiedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            };

            _context.InvestmentCategories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Category created successfully with ID {Id}", category.Id);
            return Result.Success(category.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category: {Message}", ex.Message);
            return Result.Error($"Failed to create category: {ex.Message}");
        }
    }
} 