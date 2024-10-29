namespace BaseCrud.Expressions.Filter;

public class FilterExpressions<TEntity>
{
    public readonly
        Dictionary<Expression<Func<TEntity, object>>, Func<PropertyFilterExpression<TEntity, object>, PropertyFilterExpression<TEntity, object>>>
        FilterPropertyExpressions;

    public readonly Dictionary<string, Expression<Func<TEntity, bool>>> RuleExpressions;

    public readonly Dictionary<string, Expression<Func<TEntity, object, bool>>> RuleTypedExpressions;

    internal FilterExpressions(
        Dictionary<
            Expression<Func<TEntity, object>>,
            Func<PropertyFilterExpression<TEntity, object>, PropertyFilterExpression<TEntity, object>>
        > filterExpressions,
        Dictionary<string, Expression<Func<TEntity, bool>>> ruleExpressions,
        Dictionary<string, Expression<Func<TEntity, object, bool>>> ruleTypedExpressions
    )
    {
        FilterPropertyExpressions = filterExpressions;
        RuleExpressions = ruleExpressions;
        RuleTypedExpressions = ruleTypedExpressions;
    }

    public bool HasFilterPropertyExpressions(string propertyName)
    {
        return FilterPropertyExpressions.Any(x => x.Key.Body.ToString().Contains(propertyName));
    }
}