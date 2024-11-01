﻿using System.Text.Json;
using System.Text.Json.Serialization;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Entities;

namespace BaseCrud.PrimeNg;

public class PrimeTableMetaData : IDataTableMetaData
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
    public IEnumerable<FilterExpressionMetaData> FilterExpressionMetaData =>
        Filters.Select(filter => new FilterExpressionMetaData(filter.Key,
            PrimeNgExtensions.ConvertToConstraintsEnum(filter.Value.Operator!), filter.Value.Value!));

    [JsonIgnore]
    public IEnumerable<SortingExpressionMetaData> SortingExpressionMetaData =>
        string.IsNullOrEmpty(SortField)
            ? []
            : [new SortingExpressionMetaData(SortField, SortOrder == 1)];

    public IEnumerable<GlobalFilterExpressionMetaData> GlobalFilterExpressionMetaData =>
        string.IsNullOrEmpty(GlobalFilter)
            ? []
            : [new GlobalFilterExpressionMetaData(GlobalFilter)];

    public IEnumerable<string> Rules =>
        Filters
            .Where(x => x.Value is { MatchMode: "rule", Value: null })
            .Select(x => x.Key);

    public IEnumerable<(string, object)> RuleFilters =>
        Filters
            .Where(x => x.Value is { MatchMode: "rule", Value: not null })
            .Select(x => (x.Key, x.Value.Value!));
}