using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;

namespace Application.Features.InvestmentCategories.GetCategoryById;

public record GetCategoryByIdRequest : IRequest<Result<CategoryDto>>
{
    public int Id { get; init; }
}

public record CategoryDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdRequest, Result<CategoryDto>>
{
    private readonly IContext _context;

    public GetCategoryByIdHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.InvestmentCategories.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (category == null)
        {
            return Result.NotFound("Category not found");
        }

        return Result.Success(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }
} 