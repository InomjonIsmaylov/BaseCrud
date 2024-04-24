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
        var filterExpressionArgs = filterExpressionMetaData as FilterExpressionMetaData[] ?? filterExpressionMetaData.ToArray();

        if (filterExpressionArgs.Length == 0)
            return queryable;

        var first = filterExpressionArgs.First();

        var resultExpression = expressionBuilder.BuildFilterExpression(first.PropertyName, first.Constraint, first.Value);

        var predicate = filterExpressionArgs.Skip(1)
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
        var sortingExpressionArgs = getSortingExpressionMetaData as SortingExpressionMetaData[] ?? getSortingExpressionMetaData.ToArray();

        if (sortingExpressionArgs.Length == 0)
            return query;

        var first = sortingExpressionArgs.First();

        var resultExpression = expressionBuilder.BuildSortExpression(first.PropertyName);

        var sortedQuery = first.Ascending
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
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(left.Body, right.Body), parameter);
    }
}