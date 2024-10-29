using BaseCrud.Expressions.Filter;
using System.Data;
using System.Reflection;
using BaseCrud.Extensions;
using BaseCrud.Expressions;

namespace BaseCrud.Internal;

internal static class CustomFilterExpressionsHandler
{
    public static Dictionary<Type, CompiledExpressionStorage> CompiledExpressions { get; } = new();

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
                FilterExpressions<object> filterExpressions = GetFilterExpressions(expressionType, entityType);

                var storage = new CompiledExpressionStorage(entityType);

                foreach ((
                             Expression<Func<object, object>> selector,
                             Func<PropertyFilterExpression<object, object>, PropertyFilterExpression<object, object>> getFiltersFunction
                        ) in filterExpressions.FilterPropertyExpressions)
                {
                    PropertyInfo prop = GetPropertyInfo(selector);

                    var pStorage = new CompiledPropertyExpressionStorage(prop);

                    var propertyFilterExpression =
                        (PropertyFilterExpression<object, object>)Activator.CreateInstance(
                            typeof(PropertyFilterExpression<,>).MakeGenericType(entityType, prop.PropertyType))!;

                    PropertyFilterExpression<object, object> result = getFiltersFunction(propertyFilterExpression);

                    Dictionary<ExpressionConstraintsEnum, Expression<Func<object, object, bool>>> filters =
                        result.Filters;

                    FillCompiledPredicate(filters, prop, pStorage, storage, entityType);

                    Dictionary<ExpressionConstraintsEnum, Expression<Func<object, object, bool>>> filtersWithOtherType =
                        result.FiltersWithOtherType;

                    FillTypedCompiledPredicate(filtersWithOtherType, prop, pStorage, storage, entityType);

                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }




    private static PropertyInfo GetPropertyInfo(Expression<Func<object, object>> selector)
    {
        var prop = (PropertyInfo)((MemberExpression)selector.Body).Member;

        return prop;
    }



    private static void FillTypedCompiledPredicate(Dictionary<ExpressionConstraintsEnum, Expression<Func<object, object, bool>>> filtersWithOtherType, PropertyInfo prop,
        CompiledPropertyExpressionStorage pStorage, CompiledExpressionStorage storage, Type entityType)
    {
        foreach ((ExpressionConstraintsEnum? constraint, Expression<Func<object, object, bool>>? predicateRaw) in filtersWithOtherType)
        {
            var compiledPredicate = new CompiledPredicate(
                Func,
                prop.PropertyType
            );

            pStorage.AddExpression(constraint, compiledPredicate);

            storage.AddProperty(prop, pStorage);

            continue;

            Expression<Func<object, bool>> Func(object filterValue)
            {
                ParameterExpression entityParam = Expression.Parameter(entityType, "entity");

                ConstantExpression constExpr = Expression.Constant(filterValue, predicateRaw.Parameters[1].Type);

                Expression<Func<object, bool>> predicate =
                    Expression.Lambda<Func<object, bool>>(
                        Expression.Invoke(predicateRaw, entityParam, constExpr), entityParam);

                return predicate;
            }
        }
    }

    private static void FillCompiledPredicate(Dictionary<ExpressionConstraintsEnum, Expression<Func<object, object, bool>>> filters, PropertyInfo prop,
        CompiledPropertyExpressionStorage pStorage, CompiledExpressionStorage storage, Type entityType)
    {
        foreach ((ExpressionConstraintsEnum? constraint, Expression<Func<object, object, bool>>? predicateRaw) in filters)
        {
            var compiledPredicate = new CompiledPredicate(
                Func,
                prop.PropertyType
            );

            pStorage.AddExpression(constraint, compiledPredicate);

            storage.AddProperty(prop, pStorage);

            continue;

            Expression<Func<object, bool>> Func(object filterValue)
            {
                ParameterExpression entityParam = Expression.Parameter(entityType, "entity");

                ConstantExpression constExpr = Expression.Constant(filterValue, prop.PropertyType);

                Expression<Func<object, bool>> predicate =
                    Expression.Lambda<Func<object, bool>>(
                        Expression.Invoke(predicateRaw, entityParam, constExpr), entityParam);

                return predicate;
            }
        }
    }

    private static FilterExpressions<object> GetFilterExpressions(Type expressionType, Type entityType)
    {
        var filterExpression = (IFilterExpression<object>?)Activator.CreateInstance(expressionType)!;

        if (filterExpression is null)
            throw new InvalidExpressionException($"can not create an object of type {expressionType}");

        Type builderType = typeof(FilterExpressionBuilder<>).MakeGenericType(entityType);

        FilterExpressionBuilder<object> builder =
            Activator.CreateInstance(builderType) as FilterExpressionBuilder<object> ??
            throw new InvalidExpressionException($"Cannot create instance of {builderType}");

        FilterExpressions<object> filterExpressions = filterExpression.FilterExpressions(builder);

        return filterExpressions;
    }
}