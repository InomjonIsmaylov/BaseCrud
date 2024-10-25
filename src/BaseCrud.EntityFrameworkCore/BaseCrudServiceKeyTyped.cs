using BaseCrud.General.Entities;

namespace BaseCrud.EntityFrameworkCore;

/// <summary>
/// <inheritdoc cref="BaseCrudService{TEntity, TDto, TDtoFull, TKey}" />
/// </summary>
public abstract class BaseCrudService<TEntity, TDto, TDtoFull>
    // Primary Constructor
    (
        DbContext dbContext,
        IMapper mapper
    )
    // Inherited Base Class
    : BaseCrudService<TEntity, TDto, TDtoFull, int>(dbContext, mapper),
        // Implementing Base Interface
        ICrudService<TEntity, TDto, TDtoFull>
    // Generic types annotations
    where TEntity : IEntity
    where TDto : class, IDataTransferObject<TEntity>
    where TDtoFull : class, IDataTransferObject<TEntity>;