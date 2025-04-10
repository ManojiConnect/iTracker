using System.Collections.Generic;
using Application.Common.Models;
using Application.Features.Investments.Common;
using MediatR;

namespace Application.Features.Investments.GetAllInvestments;

public record GetAllInvestmentsRequest : IRequest<Result<List<InvestmentDto>>>; 