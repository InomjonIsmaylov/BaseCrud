namespace BaseCrud.Entities;

public interface IDataTableMetaData
{
    PaginationMetaData PaginationMetaData { get; }

    IEnumerable<FilterExpressionMetaData> FilterExpressionMetaData { get; }

    IEnumerable<SortingExpressionMetaData> SortingExpressionMetaData { get; }

    IEnumerable<GlobalFilterExpressionMetaData> GlobalFilterExpressionMetaData { get; }

    IEnumerable<string> Rules { get; }

    IEnumerable<(string, object)> RuleFilters { get; }
}