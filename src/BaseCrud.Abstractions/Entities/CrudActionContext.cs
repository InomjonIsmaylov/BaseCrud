using AutoMapper;
using BaseCrud.General.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCrud.Abstractions.Entities;

public record CrudActionContext<TEntity, TKey>(
    IQueryable<TEntity> Queryable,
    IUserProfile UserProfile,
    DbContext DbContext,
    IMapper Mapper,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken
)
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>;

public record CrudActionContext<TEntity>(
    IQueryable<TEntity> Queryable,
    IUserProfile UserProfile,
    DbContext DbContext,
    IMapper Mapper,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken)
    : CrudActionContext<TEntity, int>(Queryable, UserProfile, DbContext, Mapper, DataTableMetaData, CancellationToken)
    where TEntity : IEntity<int>;