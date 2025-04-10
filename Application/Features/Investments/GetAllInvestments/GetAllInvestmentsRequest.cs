using System.Collections.Generic;
using Application.Common.Models;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Investments.GetAllInvestments;

public record GetAllInvestmentsRequest : IRequest<Result<PaginatedList<InvestmentDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
} 