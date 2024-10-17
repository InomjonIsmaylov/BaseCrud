using BaseCrud.General.Entities;
using System.Linq.Expressions;

namespace BaseCrud.General.Expressions;

public static class ExpressionBuilderHelper
{
    public static IQueryable<TEntity> BuildFilterExpression<TEntity>(
        this ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> queryable,
        IEnumerable<FilterExpressionMetaData> filterExpressionMetaData)
    {
        FilterExpressionMetaData[] filterExpressionArgs = filterExpressionMetaData as FilterExpressionMetaData[] ?? filterExpressionMetaData.ToArray();

        if (filterExpressionArgs.Length == 0)
            return queryable;

        FilterExpressionMetaData first = filterExpressionArgs.First();

        Expression<Func<TEntity, bool>> resultExpression = expressionBuilder.BuildFilterExpression(first.PropertyName, first.Constraint, first.Value);

        Expression<Func<TEntity, bool>> predicate = filterExpressionArgs.Skip(1)
            .Aggregate(
                resultExpression, (current, filter) =>
                    current.And(
                        expressionBuilder.BuildFilterExpression(filter.PropertyName, filter.Constraint, filter.Value)
                    )
            );

        return queryable.Where(predicate);
    }

    public static IQueryable<TEntity> BuildSortingExpression<TEntity>(
        this ExpressionBuilder<TEntity> expressionBuilder,
        IQueryable<TEntity> query,
        IEnumerable<SortingExpressionMetaData> getSortingExpressionMetaData)
    {
        SortingExpressionMetaData[] sortingExpressionArgs = getSortingExpressionMetaData as SortingExpressionMetaData[] ?? getSortingExpressionMetaData.ToArray();

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
}