using System.Reflection;

namespace BaseCrud.Internal;

internal class CompiledExpressionStorage
{
    internal CompiledExpressionStorage(Type entityType)
    {
        EntityType = entityType;
    }

    private readonly Dictionary<string, CompiledPropertyExpressionStorage> _propertyExpressions = new();

    public Type EntityType { get; }

    public void AddProperty(PropertyInfo property, CompiledPropertyExpressionStorage pExpStorage)
    {
        _propertyExpressions[property.Name] = pExpStorage;
    }

    //public Expression<Func<object, bool>>? GetExpression(string propertyName, ExpressionConstraintsEnum constraint)
    //{
    //    return _propertyExpressions.GetValueOrDefault(propertyName)?.GetExpression(constraint);
    //}
}