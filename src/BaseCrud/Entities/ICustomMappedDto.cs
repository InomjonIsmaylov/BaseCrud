using AutoMapper;

namespace BaseCrud.Entities;

/// <summary>
/// Type used to provide an opportunity to further configure the mapping of
/// <typeparamref name="TEntity"/> to <typeparamref name="TDto"/> (<em>
/// which has to be <see cref="IDataTransferObject{TEntity,TKey}"/></em>) and
/// <strong>vice versa</strong> using the <see cref="IMappingExpression{TSource,TDestination}"/>
/// of the <see href="https://github.com/AutoMapper/AutoMapper"/> library.
/// </summary>
/// <typeparam name="TEntity">Entity type of the Database</typeparam>
/// <typeparam name="TKey">Type param of Key property</typeparam>
/// <typeparam name="TDto">The Dto type (<c>typeof(this)</c>)</typeparam>
public interface ICustomMappedDto<TEntity, TKey, TDto> where TEntity : IEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TDto : IDataTransferObject<TEntity, TKey>
{
    static abstract IMappingExpression<TEntity, TDto> MapEntityToDto(IMappingExpression<TEntity, TDto> mappingExpression);

    static abstract IMappingExpression<TDto, TEntity> MapDtoToEntity(IMappingExpression<TDto, TEntity> mappingExpression);
}

/// <inheritdoc cref="ICustomMappedDto{TEntity,TKey,TDto}"/>
public interface ICustomMappedDto<TEntity, TDto> : ICustomMappedDto<TEntity, int, TDto>
    where TEntity : IEntity
    where TDto : IDataTransferObject<TEntity>;