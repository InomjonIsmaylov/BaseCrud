namespace BaseCrud.EntityFrameworkCore.Extensions;

public static class DataTableMetaDataExtensions
{
    /// <exception cref="TableMetaDataInvalidException"></exception>
    public static void ThrowIfValid(this IDataTableMetaData dataTable)
    {
        if (dataTable is null)
            throw new TableMetaDataInvalidException("DataTableMetaData is null");

        if (dataTable.PaginationMetaData.Rows <= 0)
            throw new TableMetaDataInvalidException("Rows must be greater than 0");

        if (dataTable.PaginationMetaData.First < 0)
            throw new TableMetaDataInvalidException("First must be greater than or equal to 0");
    }
}