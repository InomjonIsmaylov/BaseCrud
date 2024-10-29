namespace BaseCrud.Expressions.Filter;

public interface IFilterExpression<TEntity>
{
    Func<FilterExpressionBuilder<TEntity>, FilterExpressions<TEntity>> FilterExpressions { get; }
}