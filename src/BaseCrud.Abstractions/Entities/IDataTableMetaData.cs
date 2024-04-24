using BaseCrud.General.Entities;

namespace BaseCrud.Abstractions.Entities;

public interface IDataTableMetaData
{
    PaginationMetaData PaginationMetaData { get; }

    IEnumerable<FilterExpressionMetaData> FilterExpressionMetaData { get; }

    IEnumerable<SortingExpressionMetaData> SortingExpressionMetaData { get; }

    IEnumerable<GlobalFilterExpressionMetaData> GlobalFilterExpressionMetaData { get; }
}