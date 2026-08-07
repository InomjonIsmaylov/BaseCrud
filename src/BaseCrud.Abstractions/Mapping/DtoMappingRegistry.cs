using BaseCrud.Abstractions.Expressions;
using BaseCrud.Entities;

namespace BaseCrud.Abstractions.Mapping;

public sealed class DtoMappingRegistry : IDtoMappingRegistry
{
    private readonly Dictionary<(Type, Type), object> _entries = new();

    public void Add<TEntity, TDto, TKey>(IDtoMapping<TEntity, TDto, TKey> mapping)
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>
    {
        var key = (typeof(TEntity), typeof(TDto));
        _entries[key] = new DtoMappingEntry<TEntity, TDto, TKey>
        {
            SelectExpression = mapping.SelectExpression,
            MapToDto = mapping.SelectExpression.Compile(),
            InsertToEntity = mapping.InsertMappingToEntity.Compile(),
            UpdateEntity = mapping.UpdateMappingToEntity.Compile()
        };
    }

    public DtoMappingEntry<TEntity, TDto, TKey> Get<TEntity, TDto, TKey>()
        where TKey : struct, IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
        where TDto : class, IDataTransferObject<TEntity, TKey>
    {
        if (_entries.TryGetValue((typeof(TEntity), typeof(TDto)), out var entry))
            return (DtoMappingEntry<TEntity, TDto, TKey>)entry;

        throw new MissingDtoMappingException(typeof(TEntity), typeof(TDto), serviceType: null);
    }
}
