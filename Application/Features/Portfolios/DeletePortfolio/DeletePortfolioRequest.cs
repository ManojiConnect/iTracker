using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using MediatR;

namespace Application.Features.Portfolios.DeletePortfolio;

public record DeletePortfolioRequest : IRequest<Result<int>>
{
    public int Id { get; init; }
}

public class DeletePortfolioHandler : IRequestHandler<DeletePortfolioRequest, Result<int>>
{
    private readonly IContext _context;

    public DeletePortfolioHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(DeletePortfolioRequest request, CancellationToken cancellationToken)
    {
        var portfolio = await _context.Portfolios.FindAsync(request.Id);
        if (portfolio == null)
        {
            return Result<int>.Error("Portfolio not found");
        }

        _context.Portfolios.Remove(portfolio);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(portfolio.Id);
    }
} 