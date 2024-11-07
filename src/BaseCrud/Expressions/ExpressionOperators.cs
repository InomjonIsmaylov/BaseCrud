using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using BaseCrud.Internal;

namespace BaseCrud.Expressions;

public class ExpressionBuilder<TEntity>
{
    private readonly ParameterExpression _parameterExpression = Expression.Parameter(typeof(TEntity), "e");

    private readonly Type _entityType = typeof(TEntity);

    public Expression<Func<TEntity, bool>> BuildFilterExpression(
        string propertyName,
        ExpressionConstraintsEnum constraint,
        object? filterValue)
    {
        ArgumentNullException.ThrowIfNull(filterValue, nameof(filterValue));

        ConstantExpression constant = Expression.Constant(filterValue);

        PropertyInfo? prop = _entityType.GetProperty(propertyName);

        if (prop is null)
            throw new ArgumentException($"Property {propertyName} not found in {typeof(TEntity).Name}");

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (CustomPropertyFilteringDefined(prop, filterValue, constraint, out Expression<Func<TEntity, bool>>? result))
        {
            return result;
        }

        return DefaultPropertyFiltering(propertyName, constraint, constant);
    }

    public Expression<Func<TEntity, bool>> BuildRuleExpression(string ruleName)
    {
        RulePredicateExpression? rule = CustomFilterExpressionsHandler.GetRule(_entityType, ruleName);

        if (rule is null)
            throw new InvalidOperationException(
                $"Rule with the name {ruleName} not defined for an entity of type {_entityType.Name}");

        Expression<Func<TEntity, bool>> actual = Expression.Lambda<Func<TEntity, bool>>(
            rule.Body,
            rule.EntityParam
        );

        return actual;
    }

    public Expression<Func<TEntity, bool>> BuildRuleFilterExpression(string ruleName, object? filterValue)
    {
        ArgumentNullException.ThrowIfNull(filterValue, nameof(filterValue));
        
        RulePredicateExpressionWithFilterParam? ruleTyped =
            CustomFilterExpressionsHandler.GetTypedRule(_entityType, ruleName);

        if (ruleTyped is null)
            throw new InvalidOperationException(
                $"Rule filter with the name {ruleName} not defined for an entity of type {_entityType.Name}");

        Type filterValueType = filterValue.GetType();

        ConstantExpression filterExpression;

        if (filterValueType != ruleTyped.FilterParam.Type)
        {
            object? convertedValue = Convert.ChangeType(filterValue, ruleTyped.FilterParam.Type, CultureInfo.CurrentCulture);

            if (convertedValue is null)
            {
                throw new InvalidOperationException(
                    $"type of filter value {filterValueType} can not be cast to type {ruleTyped.FilterParam.Type}");
            }

            filterExpression = Expression.Constant(convertedValue, ruleTyped.FilterParam.Type);
        }
        else
        {
            filterExpression = Expression.Constant(filterValue, ruleTyped.FilterParam.Type);
        }

        LambdaExpression actual = Expression.Lambda(
            ruleTyped.Body,
            ruleTyped.EntityParam,
            ruleTyped.FilterParam
        );

        Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>
        (
            Expression.Invoke(actual, _parameterExpression, filterExpression),
            _parameterExpression
        );

        return predicate;

    }

    public Expression<Func<TEntity, object>> BuildSortExpression(string propertyName)
    {
        MemberExpression property = Expression.Property(_parameterExpression, propertyName);

        return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), _parameterExpression);
    }

    private bool CustomPropertyFilteringDefined(
        PropertyInfo prop,
        object filterValue,
        ExpressionConstraintsEnum constraint,
        [NotNullWhen(true)] out Expression<Func<TEntity, bool>>? result
    )
    {
        PredicateExpressionWith2Params? expressionWith2Params =
            CustomFilterExpressionsHandler.GetExpression(_entityType, prop, constraint);

        if (expressionWith2Params is null)
        {
            result = null;

            return false;
        }

        LambdaExpression actual = Expression.Lambda(
            expressionWith2Params.Body,
            expressionWith2Params.EntityParam,
            expressionWith2Params.FilterParam
        );

        Type filterValueType = filterValue.GetType();

        ConstantExpression filterExpression;

        if (filterValueType != expressionWith2Params.FilterParam.Type)
        {
            object? convertedValue = Convert.ChangeType(filterValue, expressionWith2Params.FilterParam.Type, CultureInfo.CurrentCulture);

            if (convertedValue is null)
            {
                throw new InvalidOperationException(
                    $"type of filter filterValue {filterValueType} can not be cast to type {expressionWith2Params.FilterParam.Type}");
            }

            filterExpression = Expression.Constant(convertedValue, expressionWith2Params.FilterParam.Type);
        }
        else
        {
            filterExpression = Expression.Constant(filterValue, expressionWith2Params.FilterParam.Type);
        }

        Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>
        (
            Expression.Invoke(actual, _parameterExpression, filterExpression),
            _parameterExpression
        );

        result = predicate;

        return true;
    }

    private Expression<Func<TEntity, bool>> DefaultPropertyFiltering(string propertyName, ExpressionConstraintsEnum constraint,
        ConstantExpression constant)
    {
        MemberExpression property = Expression.Property(_parameterExpression, propertyName);

        Expression expression = constraint.Name switch
        {
            nameof(ExpressionConstraintsEnum.StartsWith) => Expression.Call(property, "StartsWith", null, constant),
            nameof(ExpressionConstraintsEnum.EndsWith) => Expression.Call(property, "EndsWith", null, constant),
            nameof(ExpressionConstraintsEnum.Contains) => Expression.Call(property, "Contains", null, constant),
            nameof(ExpressionConstraintsEnum.Equals) => Expression.Equal(property, constant),
            nameof(ExpressionConstraintsEnum.NotEquals) => Expression.NotEqual(property, constant),
            nameof(ExpressionConstraintsEnum.GreaterThan) => Expression.GreaterThan(property, constant),
            nameof(ExpressionConstraintsEnum.GreaterThanOrEqual) => Expression.GreaterThanOrEqual(property, constant),
            nameof(ExpressionConstraintsEnum.LessThan) => Expression.LessThan(property, constant),
            nameof(ExpressionConstraintsEnum.LessThanOrEqual) => Expression.LessThanOrEqual(property, constant),
            nameof(ExpressionConstraintsEnum.In) => Expression.Call(typeof(List<object>), "Contains", null, constant),
            nameof(ExpressionConstraintsEnum.NotIn) => Expression.Not(Expression.Call(typeof(List<object>), "Contains", null, constant)),
            nameof(ExpressionConstraintsEnum.IsNull) => Expression.Equal(property, Expression.Constant(null)),
            nameof(ExpressionConstraintsEnum.IsNotNull) => Expression.NotEqual(property, Expression.Constant(null)),
            nameof(ExpressionConstraintsEnum.IsEmpty) => Expression.Call(typeof(string), "IsNullOrEmpty", null, property),
            nameof(ExpressionConstraintsEnum.IsNotEmpty) => Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, property)),
            nameof(ExpressionConstraintsEnum.After) => Expression.GreaterThan(property, constant),
            nameof(ExpressionConstraintsEnum.Before) => Expression.LessThan(property, constant),
            _ => throw new ArgumentOutOfRangeException(nameof(constraint), constraint, null)
        };

        return Expression.Lambda<Func<TEntity, bool>>(expression, _parameterExpression);
    }
}