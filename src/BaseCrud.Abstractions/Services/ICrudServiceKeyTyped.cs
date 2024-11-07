namespace BaseCrud.Abstractions.Services;

/// <inheritdoc />
public interface ICrudService<TEntity, TDto, TDtoFull, TUserKey>
    : ICrudService<TEntity, TDto, TDtoFull, int, TUserKey>
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>
    where TUserKey : struct, IEquatable<TUserKey>;

/// <inheritdoc />
public interface ICrudService<TEntity, TDto, TDtoFull>
    : ICrudService<TEntity, TDto, TDtoFull, int, int>
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>;