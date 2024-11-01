using System.Reflection;

namespace BaseCrud.Internal;

internal class PropertyBasedFilterExpressions
{
    internal PropertyBasedFilterExpressions(Type entityType)
    {
        EntityType = entityType;
    }

    private readonly Dictionary<string, PropertyBasedFilterExpression> _propertyExpressions = new();

    public Type EntityType { get; }

    public void AddProperty(PropertyInfo property, PropertyBasedFilterExpression pExp)
    {
        _propertyExpressions[property.Name] = pExp;
    }

    //public Expression<Func<object, bool>>? GetExpression(string propertyName, ExpressionConstraintsEnum constraint)
    //{
    //    return _propertyExpressions.GetValueOrDefault(propertyName)?.GetExpression(constraint);
    //}
}
