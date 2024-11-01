using System.Data;
using System.Reflection;
using BaseCrud.Internal;

namespace BaseCrud.Expressions.Filter;

internal class FilterExpressionBuilder
{
    private readonly dynamic _filterExpressions;

    internal FilterExpressionBuilder(Type entityType)
    {
        Type filterExpressionsType = typeof(FilterExpressions<>).MakeGenericType(entityType);
        object? filterExpressions = Activator.CreateInstance(filterExpressionsType, true);

        _filterExpressions = filterExpressions ??
                             throw new InvalidExpressionException(
                                 $"could not create an instance of {filterExpressionsType}");

        PredicateDictionary = _filterExpressions.PredicateDictionary;
        RuleExpressions = _filterExpressions.RuleExpressions;
        RuleTypedExpressions = _filterExpressions.RuleTypedExpressions;
    }

    /// <summary>
    /// returns an instance of <see cref="FilterExpressions{TEntity}"/>
    /// </summary>
    internal dynamic GetInstance()
    {
        return _filterExpressions;
    }

    internal Dictionary<PropertyInfo, HashSet<PredicateExpressionWith2Params>> PredicateDictionary { get; }
    internal Dictionary<string, RulePredicateExpression> RuleExpressions { get; }
    internal Dictionary<string, RulePredicateExpressionWithFilterParam> RuleTypedExpressions { get; }
}

public class FilterExpressions<TEntity>
{
    internal FilterExpressions()
    {
        
    }

    internal Dictionary<PropertyInfo, HashSet<PredicateExpressionWith2Params>> PredicateDictionary = new();
    internal Dictionary<string, RulePredicateExpression> RuleExpressions = new();
    internal Dictionary<string, RulePredicateExpressionWithFilterParam> RuleTypedExpressions = new();

    public FilterExpressions<TEntity> ForProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Func<PropertyFilterExpression<TEntity, TProperty>,
            PropertyFilterExpression<TEntity, TProperty>> propFilterBuilder
    )
    {
        var property = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;

        var propertyExpressionObject = new PropertyFilterExpression<TEntity, TProperty>(property);

        propertyExpressionObject = propFilterBuilder(propertyExpressionObject);

        PredicateDictionary.Add(property, propertyExpressionObject.Predicates);

        return this;
    }

    public FilterExpressions<TEntity> AddRule(string rule, Expression<Func<TEntity, bool>> predicate)
    {
        RuleExpressions.Add(rule, new RulePredicateExpression(
            predicate.Body,
            predicate.Parameters[0]
        ));

        return this;
    }

    public FilterExpressions<TEntity> AddRule<TFilterValue>(string rule, Expression<Func<TEntity, TFilterValue, bool>> predicate)
        where TFilterValue : new()
    {
        RuleTypedExpressions.Add(rule, new RulePredicateExpressionWithFilterParam(
            predicate.Body,
            predicate.Parameters[0],
            predicate.Parameters[1]
        ));

        return this;
    }
}