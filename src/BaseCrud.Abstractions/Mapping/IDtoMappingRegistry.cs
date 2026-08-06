using System.Linq.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Mapping;

public sealed class DtoMappingEntry<TEntity, TDto, TKey>
{
    public required Expression<Func<TEntity, TDto>> SelectExpression { get; init; }
    public required Func<TEntity, TDto> MapToDto { get; init; }
    public required Func<TDto, TEntity> InsertToEntity { get; init; }
    public required Func<TEntity, TDto, TEntity> UpdateEntity { get; init; }
}

public interface IDtoMappingRegistry
{
    DtoMappingEntry<TEntity, TDto, TKey> Get<TEntity, TDto, TKey>()
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>;
}
