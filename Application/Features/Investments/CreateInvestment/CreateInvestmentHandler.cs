using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Ardalis.Result;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Investments.CreateInvestment;

public class CreateInvestmentHandler : IRequestHandler<CreateInvestmentRequest, Result<int>>
{
    private readonly IContext _context;
    private readonly ILogger<CreateInvestmentHandler> _logger;

    public CreateInvestmentHandler(IContext context, ILogger<CreateInvestmentHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateInvestmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting to handle CreateInvestmentRequest for portfolio {PortfolioId}", request.PortfolioId);
            
            // Validate portfolio exists
            var portfolio = await _context.Portfolios
                .Include(p => p.Investments)
                .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && !p.IsDelete, cancellationToken);

            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio not found: {PortfolioId}", request.PortfolioId);
                return Result.NotFound("Portfolio not found");
            }
            
            _logger.LogInformation("Found portfolio: {PortfolioId}, {PortfolioName}", portfolio.Id, portfolio.Name);

            // Validate category exists
            var category = await _context.InvestmentCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Investment category not found: {CategoryId}", request.CategoryId);
                return Result.NotFound("Investment category not found");
            }
            
            _logger.LogInformation("Found category: {CategoryId}, {CategoryName}", category.Id, category.Name);

            // Always generate a symbol from the name
            string symbolToUse = request.Name.Replace(" ", "-");

            var investment = new Investment
            {
                PortfolioId = request.PortfolioId,
                Name = request.Name,
                Symbol = symbolToUse,
                CategoryId = request.CategoryId,
                TotalInvestment = request.TotalInvestment,
                CurrentValue = request.CurrentValue,
                PurchaseDate = request.PurchaseDate,
                PurchasePrice = request.PurchasePrice,
                Notes = request.Notes,
                CreatedBy = 1,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = 1,
                ModifiedOn = DateTime.UtcNow,
                IsActive = true,
                IsDelete = false
            };

            // Calculate individual investment's gain/loss and return
            investment.UnrealizedGainLoss = investment.CurrentValue - investment.TotalInvestment;
            investment.ReturnPercentage = investment.TotalInvestment > 0 
                ? (investment.UnrealizedGainLoss / investment.TotalInvestment)
                : 0;

            _logger.LogInformation("Created new investment object with Name: {Name}, Symbol: {Symbol}, TotalInvestment: {TotalInvestment}, CurrentValue: {CurrentValue}",
                investment.Name, investment.Symbol, investment.TotalInvestment, investment.CurrentValue);

            _context.Investments.Add(investment);
            
            _logger.LogInformation("Added investment to context, saving changes...");
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Investment saved with ID: {InvestmentId}", investment.Id);

            // Update portfolio totals
            _logger.LogInformation("Updating portfolio totals...");
            portfolio.TotalValue = portfolio.Investments.Where(i => !i.IsDelete).Sum(i => i.CurrentValue);
            portfolio.TotalInvestment = portfolio.Investments.Where(i => !i.IsDelete).Sum(i => i.TotalInvestment);
            portfolio.UnrealizedGainLoss = portfolio.TotalValue - portfolio.TotalInvestment;
            portfolio.ReturnPercentage = portfolio.TotalInvestment > 0 
                ? (portfolio.UnrealizedGainLoss / portfolio.TotalInvestment)
                : 0;

            _logger.LogInformation("Updated portfolio totals: TotalValue: {TotalValue}, TotalInvestment: {TotalInvestment}",
                portfolio.TotalValue, portfolio.TotalInvestment);

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Portfolio totals saved successfully");
            
            return Result.Success(investment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating investment: {ErrorMessage}", ex.Message);
            return Result.Error($"Error creating investment: {ex.Message}");
        }
    }
} 