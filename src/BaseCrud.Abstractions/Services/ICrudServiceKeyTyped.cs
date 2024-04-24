using BaseCrud.General.Entities;

namespace BaseCrud.Abstractions.Services;

/// <summary>
///     Defines the general rules that Crud Service should implement
/// </summary>
/// <typeparam name="TEntity">Type that represents Database Entity</typeparam>
/// <typeparam name="TDto">DataTransferObjectType that maps the entity</typeparam>
/// <typeparam name="TDtoFull">
///     DataTransferObjectType that maps the entity more detailed (Can be the same as
///     <typeparamref name="TDto" />)
/// </typeparam>
public interface ICrudService<TEntity, TDto, TDtoFull>
    : ICrudService<TEntity, TDto, TDtoFull, int>
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>;