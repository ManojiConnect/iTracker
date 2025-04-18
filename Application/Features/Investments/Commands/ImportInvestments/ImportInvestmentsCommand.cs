using MediatR;
using Ardalis.Result;
using System.Collections.Generic;

namespace Application.Features.Investments.Commands.ImportInvestments;

public class ImportInvestmentsCommand : IRequest<Result<List<string>>>
{
    public string CsvContent { get; set; } = string.Empty;
    public int PortfolioId { get; set; }
    public List<int> SelectedRows { get; set; } = new();
} 