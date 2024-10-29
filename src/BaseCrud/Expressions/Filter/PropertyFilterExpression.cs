namespace BaseCrud.Expressions.Filter;

public class PropertyFilterExpression<TEntity, TProperty>
{
    internal PropertyFilterExpression()
    {
        
    }

    public Dictionary<ExpressionConstraintsEnum, Expression<Func<TEntity, TProperty, bool>>> Filters { get; } = new();

    public Dictionary<ExpressionConstraintsEnum, Expression<Func<TEntity, object, bool>>> FiltersWithOtherType { get; } = new();

    public PropertyFilterExpression<TEntity, TProperty> HasFilter(
        Expression<Func<TEntity, TProperty, bool>> predicate,
        ExpressionConstraintsEnum when)
    {
        Filters.TryAdd(when, predicate);

        return this;
    }

    public PropertyFilterExpression<TEntity, TProperty> HasFilter<TFilter>(
        Expression<Func<TEntity, TFilter, bool>> predicate,
        ExpressionConstraintsEnum when)
    {
        FiltersWithOtherType.TryAdd(when, (predicate as Expression<Func<TEntity, object, bool>>)!);

        return this;
    }
}