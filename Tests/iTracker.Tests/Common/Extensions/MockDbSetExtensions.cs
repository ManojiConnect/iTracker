using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace iTracker.Tests.Common.Extensions;

public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMock<T>(this IQueryable<T> data) where T : class
    {
        var mock = new Mock<DbSet<T>>();
        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        return mock;
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<TResult>> setup,
        TResult value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IEnumerable<TResult>>> setup,
        IEnumerable<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IList<TResult>>> setup,
        IList<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<List<TResult>>> setup,
        List<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<ICollection<TResult>>> setup,
        ICollection<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<HashSet<TResult>>> setup,
        HashSet<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<Dictionary<string, TResult>>> setup,
        Dictionary<string, TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IDictionary<string, TResult>>> setup,
        IDictionary<string, TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<ILookup<string, TResult>>> setup,
        ILookup<string, TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IGrouping<string, TResult>>> setup,
        IGrouping<string, TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IOrderedEnumerable<TResult>>> setup,
        IOrderedEnumerable<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IOrderedQueryable<TResult>>> setup,
        IOrderedQueryable<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<IQueryable<TResult>>> setup,
        IQueryable<TResult> value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<TResult[]>> setup,
        TResult[] value) where TMock : class
    {
        return setup.Returns(Task.FromResult(value));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<TResult>> setup,
        Func<TResult> valueFactory) where TMock : class
    {
        return setup.Returns(Task.FromResult(valueFactory()));
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<TResult>> setup,
        Func<Task<TResult>> valueFactory) where TMock : class
    {
        return setup.Returns(valueFactory());
    }

    public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(
        this ISetup<TMock, Task<TResult>> setup,
        Func<CancellationToken, Task<TResult>> valueFactory) where TMock : class
    {
        return setup.Returns<CancellationToken>(valueFactory);
    }

    public static void SetupQueryable<T>(this Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
    {
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })
            ?.MakeGenericMethod(resultType)
            .Invoke(_inner, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryProvider _provider;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
        _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
        _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => _provider;
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }
} 