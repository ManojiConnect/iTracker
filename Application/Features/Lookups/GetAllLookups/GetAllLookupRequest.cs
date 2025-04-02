using Ardalis.Result;
using Application.Features.Common.Responses;
using Application.Features.Lookups.Responses;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Lookups.GetAllLookups;
public class GetAllLookupRequest : IRequest<Result<PaginatedList<LookupResponse>>>
{
    public string Type { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
