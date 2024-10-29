using System.Reflection;
using BaseCrud.Expressions;

namespace BaseCrud.Internal;

internal class CompiledPropertyExpressionStorage
{
    internal CompiledPropertyExpressionStorage(PropertyInfo propertyInfo)
        => PropertyInfo = propertyInfo;

    public PropertyInfo PropertyInfo { get; }

    private readonly Dictionary<ExpressionConstraintsEnum, CompiledPredicate> _propertyExpressions =
        new();

    public void AddExpression(ExpressionConstraintsEnum constraint, CompiledPredicate compiledPredicate)
    {
        _propertyExpressions[constraint] = compiledPredicate;
    }
}