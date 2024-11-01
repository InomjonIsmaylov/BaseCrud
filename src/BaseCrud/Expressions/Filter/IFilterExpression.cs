namespace BaseCrud.Expressions.Filter;

public interface IFilterExpression<TEntity> where TEntity : new()
{
    Func<FilterExpressions<TEntity>, FilterExpressions<TEntity>> FilterExpressions { get; }
}