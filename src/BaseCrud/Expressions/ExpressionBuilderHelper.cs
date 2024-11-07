using BaseCrud.Entities;

namespace BaseCrud.Expressions;

public static class ExpressionBuilderHelper
{
    public static IQueryable<TEntity> BuildFilterExpression<TEntity>(
        this ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> queryable,
        IDataTableMetaData dataTableMeta)
    {
        queryable = HandlePropertyBasedFiltering(expressionBuilder, queryable, dataTableMeta.FilterExpressionMetaData);

        queryable = HandleRuleBasedFiltering(expressionBuilder, queryable, dataTableMeta.Rules);

        queryable = HandleRuleFilterBasedFiltering(expressionBuilder, queryable, dataTableMeta.RuleFilters);

        return queryable;
    }

    public static IQueryable<TEntity> BuildSortingExpression<TEntity>(
        this ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> query,
        IEnumerable<SortingExpressionMetaData> sortingExpressionMetaData)
    {
        SortingExpressionMetaData[] sortingExpressionArgs = sortingExpressionMetaData as SortingExpressionMetaData[]
                                                            ?? sortingExpressionMetaData.ToArray();

        foreach ((string? original, bool _) in sortingExpressionArgs)
        {
            if (typeof(TEntity).GetProperty(original) is null)
                throw new ArgumentException($"Property {original} not found in {typeof(TEntity).Name}");
        }

        if (sortingExpressionArgs.Length == 0)
            return query;

        SortingExpressionMetaData first = sortingExpressionArgs.First();

        Expression<Func<TEntity, object>> resultExpression = expressionBuilder.BuildSortExpression(first.PropertyName);

        IOrderedQueryable<TEntity> sortedQuery = first.Ascending
            ? query.OrderBy(resultExpression)
            : query.OrderByDescending(resultExpression);

        sortedQuery = sortingExpressionArgs.Skip(1)
            .Aggregate(sortedQuery, (current, sort) =>
                sort.Ascending
                    ? current.ThenBy(expressionBuilder.BuildSortExpression(sort.PropertyName))
                    : current.ThenByDescending(expressionBuilder.BuildSortExpression(sort.PropertyName))
            );

        return sortedQuery;
    }

    public static Expression<Func<TEntity, bool>> And<TEntity>(
        this Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");

        return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(left.Body, right.Body), parameter);
    }

    private static IQueryable<TEntity> HandlePropertyBasedFiltering<TEntity>(
        ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> queryable,
        IEnumerable<FilterExpressionMetaData> filterExpressionMetaData)
    {
        FilterExpressionMetaData[] filterExpressionArgs =
            filterExpressionMetaData as FilterExpressionMetaData[] ?? filterExpressionMetaData.ToArray();

        if (filterExpressionArgs.Length <= 0)
            return queryable;

        FilterExpressionMetaData first = filterExpressionArgs.First();

        Expression<Func<TEntity, bool>> resultExpression =
            expressionBuilder.BuildFilterExpression(first.PropertyName, first.Constraint, first.Value);

        Expression<Func<TEntity, bool>> predicate = filterExpressionArgs.Skip(1)
            .Aggregate(
                resultExpression, (current, filter) =>
                    current.And(
                        expressionBuilder.BuildFilterExpression(filter.PropertyName, filter.Constraint, filter.Value)
                    )
            );

        queryable = queryable.Where(predicate);

        return queryable;
    }
    
    private static IQueryable<TEntity> HandleRuleBasedFiltering<TEntity>(
        ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> queryable,
        IEnumerable<string> rules)
    {
        string[] ruleArray = rules as string[] ?? rules.ToArray();

        if (ruleArray.Length == 0)
            return queryable;

        string first = ruleArray[0];

        Expression<Func<TEntity, bool>> resultExpression =
            expressionBuilder.BuildRuleExpression(first);

        Expression<Func<TEntity, bool>> predicate = ruleArray.Skip(1)
            .Aggregate(
                resultExpression, (current, rule) =>
                    current.And(
                        expressionBuilder.BuildRuleExpression(rule)
                    )
            );

        queryable = queryable.Where(predicate);

        return queryable;
    }

    private static IQueryable<TEntity> HandleRuleFilterBasedFiltering<TEntity>(
        ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> queryable,
        IEnumerable<(string, object)> ruleFilters)
    {
        (string, object)[] ruleFilterArray = ruleFilters as (string, object)[] ?? ruleFilters.ToArray();

        if (ruleFilterArray.Length == 0)
            return queryable;

        (string ruleName, object filterValue) = ruleFilterArray[0];

        Expression<Func<TEntity, bool>> resultExpression =
            expressionBuilder.BuildRuleFilterExpression(ruleName, filterValue);

        Expression<Func<TEntity, bool>> predicate = ruleFilterArray.Skip(1)
            .Aggregate(
                resultExpression, (current, kvp) =>
                    current.And(
                        expressionBuilder.BuildRuleFilterExpression(kvp.Item1, kvp.Item2)
                    )
            );

        queryable = queryable.Where(predicate);

        return queryable;
    }
}