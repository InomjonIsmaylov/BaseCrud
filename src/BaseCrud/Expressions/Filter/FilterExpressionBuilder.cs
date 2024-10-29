namespace BaseCrud.Expressions.Filter;

public class FilterExpressionBuilder<TEntity> : IDisposable
{
    internal FilterExpressionBuilder()
    {
        
    }

    private readonly Dictionary<Expression<Func<TEntity, object>>, Func<
        PropertyFilterExpression<TEntity, object>,
        PropertyFilterExpression<TEntity, object>>> _propertyExpressions = new();

    private readonly Dictionary<string, Expression<Func<TEntity, bool>>> _ruleExpressions = new();
    private readonly Dictionary<string, Expression<Func<TEntity, object, bool>>> _ruleTypedExpressions = new();

    public FilterExpressionBuilder<TEntity> ForProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Func<PropertyFilterExpression<TEntity, TProperty>,
            PropertyFilterExpression<TEntity, TProperty>> propFilterBuilder
    )
    {
        var key = (propertyExpression as Expression<Func<TEntity, object>>)!;

        var value =
            (propFilterBuilder as Func<PropertyFilterExpression<TEntity, object>,
                PropertyFilterExpression<TEntity, object>>)!;

        _propertyExpressions.Add(key, value);

        return this;
    }

    public FilterExpressionBuilder<TEntity> AddRule(string rule, Expression<Func<TEntity, bool>> predicate)
    {
        _ruleExpressions.Add(rule, predicate);

        return this;
    }

    public FilterExpressionBuilder<TEntity> AddRule<TFilterValue>(string rule, Func<TEntity, TFilterValue, bool> predicate)
    {
        _ruleTypedExpressions.Add(rule, (predicate as Expression<Func<TEntity, object, bool>>)!);

        return this;
    }

    public FilterExpressions<TEntity> Build()
    {
        return new FilterExpressions<TEntity>(
            _propertyExpressions,
            _ruleExpressions,
            _ruleTypedExpressions
        );
    }
    
    public void Dispose()
    {
        _propertyExpressions.Clear();

        _ruleExpressions.Clear();

        _ruleTypedExpressions.Clear();

        GC.SuppressFinalize(this);
    }
}