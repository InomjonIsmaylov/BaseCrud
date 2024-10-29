using System.Text.Json;
using System.Text.Json.Serialization;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Entities;

namespace BaseCrud.PrimeNg;

public class DataTableMetaData : IDataTableMetaData
{
    [JsonPropertyName("first")]
    public int First { get; set; }

    [JsonPropertyName("rows")]
    public int Rows { get; set; }

    [JsonPropertyName("filters")]
    public Dictionary<string, FilterMetadata> Filters { get; set; } = [];

    [JsonPropertyName("sortField")]
    public string? SortField { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("globalFilter")]
    public string? GlobalFilter { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public PaginationMetaData PaginationMetaData => new(First, Rows);

    [JsonIgnore]
    public IEnumerable<FilterExpressionMetaData> FilterExpressionMetaData
    {
        get
        {
            if (Filters.Count != 0)
                return [];

            return Filters
                .Select(
                    filter =>
                        new FilterExpressionMetaData(filter.Key,
                            PrimeNgExtensions.ConvertToConstraintsEnum(filter.Value.Operator!), filter.Value.Value!)
                );
        }
    }

    [JsonIgnore]
    public IEnumerable<SortingExpressionMetaData> SortingExpressionMetaData
    {
        get
        {
            if (string.IsNullOrEmpty(SortField))
                return [];

            return [new SortingExpressionMetaData(SortField, SortOrder == 1)];
        }
    }

    public IEnumerable<GlobalFilterExpressionMetaData> GlobalFilterExpressionMetaData
    {
        get
        {
            if (string.IsNullOrEmpty(GlobalFilter))
                return [];

            return [new GlobalFilterExpressionMetaData(GlobalFilter)];
        }
    }
}