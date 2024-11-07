using System.Data;
using System.Reflection;
using BaseCrud.Internal;

namespace BaseCrud.Expressions.Filter;

public class PropertyFilterExpression<TEntity, TProperty>
{
    private readonly PropertyInfo _property;

    internal PropertyFilterExpression(PropertyInfo property)
    {
        _property = property;
    }

    internal HashSet<PredicateExpressionWith2Params> Predicates = [];

    public PropertyFilterExpression<TEntity, TProperty> HasFilter(
        Expression<Func<TEntity, TProperty, bool>> predicate,
        ExpressionConstraintsEnum when)
    {
        if (Predicates.Any(p => p.WhenEnum.Name == when.Name))
            throw new InvalidExpressionException(
                $"Predicate for property {_property.Name} of {typeof(TEntity).Name} with condition {when.Name} has already been registered");

        Predicates.Add(
            new PredicateExpressionWith2Params(
                when,
                predicate.Body,
                predicate.Parameters[0],
                FilterParam: predicate.Parameters[1])
        );

        return this;
    }

    public PropertyFilterExpression<TEntity, TProperty> HasFilter<TFilter>(
        Expression<Func<TEntity, TFilter, bool>> predicate,
        ExpressionConstraintsEnum when)
    {
        Predicates.Add(
            new PredicateExpressionWith2Params(
                when,
                predicate.Body,
                predicate.Parameters[0],
                FilterParam: predicate.Parameters[1])
        );

        return this;
    }
}