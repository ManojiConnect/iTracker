namespace Application.Features.Common.Requests;

public record PaginatedRequest
{
    public int CurrentPage { get; init; } = 1;

    public int PageSize { get; init; } = 10;
    public bool Paging { get; set; } = false;
}