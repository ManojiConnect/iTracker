using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Common.Responses;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}

public static class PaginatedListHelper
{

    public const int DefaultPageSize = 15;
    public const int DefaultCurrentPage = 1;

    public static async Task<PaginatedList<TDestination>> ToPaginatedListAsync<TSource, TDestination>(this IQueryable<TSource> source, int currentPage, int pageSize, bool paging)
    {
        currentPage = currentPage > 0 ? currentPage : DefaultCurrentPage;
        pageSize = pageSize > 0 ? pageSize : DefaultPageSize;
        var count = await source.CountAsync();
        if (paging)
        {
            var items = await source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
            var data = TypeAdapter.Adapt<List<TDestination>>(items);
            return new PaginatedList<TDestination>(data, count, currentPage, pageSize);
        }
        else
        {
            var data = TypeAdapter.Adapt<List<TDestination>>(source);
            return new PaginatedList<TDestination>(data, count, currentPage, pageSize);
        }
    }

    public static async Task<PaginatedList<TDestination>> ToPaginatedListAsync<TSource, TDestination>(this IEnumerable<TSource> source, int currentPage, int pageSize, bool paging)
    {
        currentPage = currentPage > 0 ? currentPage : DefaultCurrentPage;
        pageSize = pageSize > 0 ? pageSize : DefaultPageSize;
        var count = source.Count();
        if (paging)
        {
            var items = source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var data = TypeAdapter.Adapt<List<TDestination>>(items);
            return new PaginatedList<TDestination>(data, count, currentPage, pageSize);
        }
        else
        {
            var data = TypeAdapter.Adapt<List<TDestination>>(source);
            return new PaginatedList<TDestination>(data, count, currentPage, pageSize);
        }
    }
}