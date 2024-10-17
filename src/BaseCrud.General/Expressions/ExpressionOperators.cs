using System.Linq.Expressions;

namespace BaseCrud.General.Expressions;

public class ExpressionBuilder<TEntity>
{
    private readonly ParameterExpression _parameterExpression = Expression.Parameter(typeof(TEntity), "e");

    public Expression<Func<TEntity, bool>> BuildFilterExpression(
        string propertyName,
        ExpressionConstraintsEnum constraint,
        object value)
    {
        if (typeof(TEntity).GetFieldOrProperty(propertyName) is null)
            throw new ArgumentException($"Property {propertyName} not found in {typeof(TEntity).Name}");

        var constant = Expression.Constant(value);

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
        var property = Expression.Property(_parameterExpression, propertyName);

        return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), _parameterExpression);
    }
}