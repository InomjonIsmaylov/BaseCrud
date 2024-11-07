using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Entities;

namespace BaseCrud.PrimeNg;

public class PrimeTableMetaData : IDataTableMetaData
{
    [JsonPropertyName("first")]
    public int First { get; set; }

    [JsonPropertyName("rows")]
    [DefaultValue(10)]
    public int Rows { get; set; }

    [JsonPropertyName("filters")]
    public Dictionary<string, FilterMetadata> Filters { get; set; } = new();

    [JsonPropertyName("sortField")]
    [DefaultValue("id")]
    public string? SortField { get; set; }

    [JsonPropertyName("sortOrder")]
    [DefaultValue(1)]
    public int SortOrder { get; set; }

    [JsonPropertyName("globalFilter")]
    [DefaultValue("")]
    public string? GlobalFilter { get; set; }

    public override string ToString()
    {
        
        return JsonSerializer.Serialize(this);
    }

    [JsonIgnore]
    public PaginationMetaData PaginationMetaData => new(First, Rows);

    [JsonIgnore]
    public IEnumerable<FilterExpressionMetaData> FilterExpressionMetaData =>
        Filters.Where(f => f.Value.MatchMode != RuleIdentifierName).Select(filter => new FilterExpressionMetaData(filter.Key,
            PrimeNgExtensions.ConvertToConstraintsEnum(filter.Value.MatchMode!), filter.Value.Value!));

    [JsonIgnore]
    public IEnumerable<SortingExpressionMetaData> SortingExpressionMetaData =>
        string.IsNullOrEmpty(SortField)
            ? []
            : [new SortingExpressionMetaData(SortField, SortOrder == 1)];

    [JsonIgnore]
    public IEnumerable<GlobalFilterExpressionMetaData> GlobalFilterExpressionMetaData =>
        string.IsNullOrEmpty(GlobalFilter)
            ? []
            : [new GlobalFilterExpressionMetaData(GlobalFilter)];

    [JsonIgnore]
    public IEnumerable<string> Rules =>
        Filters
            .Where(x => x.Value is { MatchMode: RuleIdentifierName, Value: null })
            .Select(x => x.Key);

    [JsonIgnore]
    public IEnumerable<(string, object)> RuleFilters =>
        Filters
            .Where(x => x.Value is { MatchMode: RuleIdentifierName, Value: not null })
            .Select(x => (x.Key, x.Value.Value!));

    [JsonIgnore]
    public const string RuleIdentifierName = "rule";
}