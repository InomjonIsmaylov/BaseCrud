namespace BaseCrud.EntityFrameworkCore;

/// <summary>
/// <inheritdoc cref="BaseCrudService{TEntity, TDto, TDtoFull, TKey}" />
/// </summary>
public abstract class BaseCrudService<TEntity, TDto, TDtoFull, TUserKey>(
    DbContext dbContext,
    IMapper mapper
)
    : BaseCrudService<TEntity, TDto, TDtoFull, int, TUserKey>(
        dbContext,
        mapper
    ), ICrudService<TEntity, TDto, TDtoFull, TUserKey>
        where TEntity : class, IEntity
        where TDto : class, IDataTransferObject<TEntity>
        where TDtoFull : class, IDataTransferObject<TEntity>
        where TUserKey : struct, IEquatable<TUserKey>;

/// <summary>
/// <inheritdoc cref="BaseCrudService{TEntity, TDto, TDtoFull, TKey}" />
/// </summary>
public abstract class BaseCrudService<TEntity, TDto, TDtoFull>(
    DbContext dbContext,
    IMapper mapper
)
    : BaseCrudService<TEntity, TDto, TDtoFull, int, int>(
        dbContext,
        mapper
    ), ICrudService<TEntity, TDto, TDtoFull>
    where TEntity : class, IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>;