using System.Reflection;
using BaseCrud.Extensions;
using BaseCrud.Internal;

namespace BaseCrud.Expressions;

// TODO: implement custom expression binding and predicate as a rule mechanism
public class ExpressionBuilder<TEntity>
{
    private readonly ParameterExpression _parameterExpression = Expression.Parameter(typeof(TEntity), "e");

    public Expression<Func<TEntity, bool>> BuildFilterExpression(
        string propertyName,
        ExpressionConstraintsEnum constraint,
        object value)
    {
        string ruleName = propertyName;
        propertyName = propertyName.Capitalize()!;

        Type entityType = typeof(TEntity);

        ConstantExpression constant = Expression.Constant(value);

        PropertyInfo? prop = entityType.GetProperty(propertyName);

        if (prop != null)
        {
            PredicateExpressionWith2Params? expressionWith2Params =
                CustomFilterExpressionsHandler.GetExpression(entityType, prop, constraint);

            if (expressionWith2Params is not null)
            {
                LambdaExpression actual = Expression.Lambda(
                    expressionWith2Params.Body,
                    expressionWith2Params.EntityParam,
                    expressionWith2Params.FilterParam
                );

                // TODO: type should be cast if value param type is not of expressionWith2Params.FilterParam.Type type
                ConstantExpression filterExpression =
                    Expression.Constant(value, expressionWith2Params.FilterParam.Type);

                Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>
                (
                    Expression.Invoke(actual, _parameterExpression, filterExpression),
                    _parameterExpression
                );

                return predicate;
            }
        }

        RulePredicateExpression? rule = CustomFilterExpressionsHandler.GetRule(entityType, ruleName);

        if (rule is not null)
        {
            Expression<Func<TEntity, bool>> actual = Expression.Lambda<Func<TEntity, bool>>(
                rule.Body,
                rule.EntityParam
            );

            return actual;
        }

        RulePredicateExpressionWithFilterParam? ruleTyped = CustomFilterExpressionsHandler.GetTypedRule(entityType, ruleName);

        if (ruleTyped is not null)
        {
            if (value.GetType() != ruleTyped.FilterParam.Type)
            {
                var convertedValue = (object?)Convert.ChangeType(value, ruleTyped.FilterParam.Type);
                if (convertedValue is null)
                {

                }
            }

            LambdaExpression actual = Expression.Lambda(
                ruleTyped.Body,
                ruleTyped.EntityParam,
                ruleTyped.FilterParam
            );

            // TODO: type should be cast if value param type is not of expressionWith2Params.FilterParam.Type type
            ConstantExpression filterExpression = Expression.Constant(value, ruleTyped.FilterParam.Type);

            Expression<Func<TEntity, bool>> predicate = Expression.Lambda<Func<TEntity, bool>>
            (
                Expression.Invoke(actual, _parameterExpression, filterExpression),
                _parameterExpression
            );

            return predicate;
        }

        // TODO: instead of throwing an exception, check if the property name (as rule name) exists in the provided custom rules as well
        if (prop is null)
            throw new ArgumentException($"Property {propertyName} not found in {typeof(TEntity).Name}");

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

    public Expression<Func<TEntity, object>> BuildSortExpression(string propertyName)
    {
        MemberExpression property = Expression.Property(_parameterExpression, propertyName);

        return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), _parameterExpression);
    }
}