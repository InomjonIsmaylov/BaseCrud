namespace BaseCrud.Abstractions.Expressions;

public interface ISelectExpression<TEntity, TDto>
    : ISelectExpression<TEntity, TDto, int>
    where TEntity : IEntity
    where TDto : IDataTransferObject<TEntity, int>;

public interface ISelectExpression<TEntity, TDto, in TKey>
    where TKey : struct, IEquatable<TKey>
    where TEntity : IEntity<TKey>
    where TDto : IDataTransferObject<TEntity, TKey>
{
    Expression<Func<TEntity, TDto>> SelectExpression { get; }
}
