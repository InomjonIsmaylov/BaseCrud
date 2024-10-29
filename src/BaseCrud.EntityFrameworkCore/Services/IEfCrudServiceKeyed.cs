namespace BaseCrud.EntityFrameworkCore.Services;

/// <inheritdoc />
public interface IEfCrudService<TEntity, TDto, TDtoFull, TUserKey>
    : IEfCrudService<TEntity, TDto, TDtoFull, int, TUserKey>
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>
    where TUserKey : struct, IEquatable<TUserKey>;

/// <inheritdoc />
public interface IEfCrudService<TEntity, TDto, TDtoFull>
    : IEfCrudService<TEntity, TDto, TDtoFull, int, int>
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>;