using AutoMapper;

namespace BaseCrud.Abstractions.Entities;

public record CrudActionContext<TEntity, TKey, TUserKey>(
    IQueryable<TEntity> Queryable,
    IUserProfile<TUserKey>? UserProfile,
    IMapper Mapper,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken
)
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TUserKey : struct, IEquatable<TUserKey>;

public record CrudActionContext<TEntity, TUserKey>(
    IQueryable<TEntity> Queryable,
    IUserProfile<TUserKey>? UserProfile,
    IMapper Mapper,
    IDataTableMetaData? DataTableMetaData,
    CancellationToken CancellationToken
)
    : CrudActionContext<TEntity, int, TUserKey>(
        Queryable,
        UserProfile,
        Mapper,
        DataTableMetaData,
        CancellationToken
    )
    where TEntity : IEntity<int>
    where TUserKey : struct, IEquatable<TUserKey>;