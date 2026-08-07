using System.Linq.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Expressions;

public interface IDtoMapping<TEntity, TDto> : IDtoMapping<TEntity, TDto, int>
    where TEntity : IEntity
    where TDto : IDataTransferObject<TEntity, int>;

public interface IDtoMapping<TEntity, TDto, TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : IDataTransferObject<TEntity, TKey>
{
    /// <summary>EF-translatable projection; also compiled for in-memory Entity→DTO.</summary>
    Expression<Func<TEntity, TDto>> SelectExpression { get; }

    /// <summary>In-memory only. Builds a new entity for insert.</summary>
    Expression<Func<TDto, TEntity>> InsertMappingToEntity { get; }

    /// <summary>In-memory only. Produces updated entity values from existing + dto.</summary>
    Expression<Func<TEntity, TDto, TEntity>> UpdateMappingToEntity { get; }
}
