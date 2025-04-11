using System.Collections.Generic;
using Application.Common.Models;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Common.Models;

namespace Application.Features.Investments.GetAllInvestments;

public class GetAllInvestmentsRequest : IRequest<Result<PaginatedList<InvestmentDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchText { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
    public string? Category { get; set; }
    public int? PortfolioId { get; set; }
} 