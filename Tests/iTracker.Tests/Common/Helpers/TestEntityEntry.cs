using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace iTracker.Tests.Common.Helpers;

public class TestEntityEntry<TEntity> : EntityEntry<TEntity> where TEntity : class
{
    private readonly TEntity _entity;
    private EntityState _state;

    public TestEntityEntry(TEntity entity) : base(null!)
    {
        _entity = entity;
        _state = EntityState.Added;
    }

    public override TEntity Entity => _entity;
    public override EntityState State
    {
        get => _state;
        set => _state = value;
    }
} 