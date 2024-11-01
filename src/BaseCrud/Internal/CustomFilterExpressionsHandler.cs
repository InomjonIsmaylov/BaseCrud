using BaseCrud.Expressions.Filter;
using System.Data;
using System.Reflection;
using BaseCrud.Expressions;
using BaseCrud.Extensions;

namespace BaseCrud.Internal;

internal static class CustomFilterExpressionsHandler
{
    private static readonly Dictionary<Type, Dictionary<PropertyInfo, HashSet<PredicateExpressionWith2Params>>> CompiledExpressions = new();

    private static readonly Dictionary<Type, Dictionary<string, RulePredicateExpression>> CompiledRuleExpressions = new();

    private static readonly Dictionary<Type, Dictionary<string, RulePredicateExpressionWithFilterParam>> CompiledTypedRuleExpressions = new();

    /// <summary>
    /// Scans the given assembly to register Custom filter expressions (<see cref="IFilterExpression{TEntity}"/>)
    /// </summary>
    public static void Scan(Assembly assembly)
    {
        try
        {
            IEnumerable<(Type, Type)> filterExpressionTypes = assembly
                .GetImplementingTypeWithGenericArguments(typeof(IFilterExpression<>))
                .Select(tuple => (tuple.Item1, tuple.Item2[0]));

            (Type, Type)[] filterExpressionTypeArray =
                filterExpressionTypes as (Type, Type)[] ?? filterExpressionTypes.ToArray();

            foreach ((Type expressionType, Type entityType) in filterExpressionTypeArray)
            {
                GetFilterExpressions(expressionType, entityType);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static PredicateExpressionWith2Params? GetExpression(Type entityType, PropertyInfo prop, ExpressionConstraintsEnum condition)
    {
        if (!CompiledExpressions.TryGetValue(entityType, out var dict))
            return null;

        return dict.TryGetValue(prop, out var hashSet)
            ? hashSet.FirstOrDefault(x => x.WhenEnum.Name == condition.Name)
            : null;
    }

    public static RulePredicateExpression? GetRule(Type entityType, string rule)
    {
        return !CompiledRuleExpressions.TryGetValue(entityType, out var dict)
            ? null
            : dict.GetValueOrDefault(rule);
    }

    public static RulePredicateExpressionWithFilterParam? GetTypedRule(Type entityType, string rule)
    {
        return !CompiledTypedRuleExpressions.TryGetValue(entityType, out var dict)
            ? null
            : dict.GetValueOrDefault(rule);
    }

    private static void GetFilterExpressions(Type expressionType, Type entityType)
    {
        dynamic? filterExpression = Activator.CreateInstance(expressionType);

        if (filterExpression is null)
            throw new InvalidExpressionException(
                $"Can not create an object of type {expressionType}. Filter expression class must have a parameterless constructor");

        var builder = new FilterExpressionBuilder(entityType);

        dynamic? filterExpressions = filterExpression.FilterExpressions(builder.GetInstance());

        if (filterExpressions is null)
            throw new InvalidExpressionException(
                $"FilterExpressions Property of {expressionType} must be a valid Function");

        Dictionary<PropertyInfo, HashSet<PredicateExpressionWith2Params>> predicateDictionary = filterExpressions.PredicateDictionary;
        Dictionary<string, RulePredicateExpression> ruleExpressions = filterExpressions.RuleExpressions;
        Dictionary<string, RulePredicateExpressionWithFilterParam> ruleTypedExpressions = filterExpressions.RuleTypedExpressions;
        
        CompiledExpressions.Add(entityType, predicateDictionary);
        CompiledRuleExpressions.Add(entityType, ruleExpressions);
        CompiledTypedRuleExpressions.Add(entityType, ruleTypedExpressions);
    }
}