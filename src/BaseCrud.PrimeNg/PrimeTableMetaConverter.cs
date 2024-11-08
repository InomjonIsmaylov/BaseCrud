using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseCrud.Extensions;

namespace BaseCrud.PrimeNg;

public class FilterMetadataConverter : JsonConverter<FilterMetadata>
{
    public override FilterMetadata? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = JsonSerializer.Deserialize<FilterMetadata>(ref reader);

        if (result is null)
            return result;

        if (result.Value is not JsonElement jsonElement)
            return result;

        string rawText = jsonElement.GetRawText();

        result.Value = jsonElement switch
        {
            { ValueKind: JsonValueKind.Null } or { ValueKind: JsonValueKind.Undefined } => null,
            { ValueKind: JsonValueKind.False } => false,
            { ValueKind: JsonValueKind.True } => true,
            { ValueKind: JsonValueKind.Number } when int.TryParse(rawText, out int resultInt) => resultInt,
            { ValueKind: JsonValueKind.Number } => jsonElement.GetDouble(),
            { ValueKind: JsonValueKind.String } when DateTime.TryParse(rawText, CultureInfo.CurrentCulture,
                out DateTime resultDateTime) => resultDateTime,
            { ValueKind: JsonValueKind.String } => jsonElement.GetString(),
            _ => throw new ArgumentOutOfRangeException(nameof(typeToConvert))
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, FilterMetadata value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}

public class PrimeTableMetaConverter : JsonConverter<PrimeTableMetaData>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new FilterMetadataConverter() }
    };

    public override PrimeTableMetaData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = JsonSerializer.Deserialize<PrimeTableMetaData>(ref reader, Options);

        if (result?.Filters == null)
            return result;

        var listOfKeys = new List<string>(result.Filters.Count);

        listOfKeys.AddRange(from filterMetadata in result.Filters
                            where filterMetadata.Value.MatchMode != "rule"
                            select filterMetadata.Key);

        foreach (string key in listOfKeys)
        {
            FilterMetadata value = result.Filters[key];

            result.Filters.Remove(key);

            string newKey = key.Capitalize();

            result.Filters.Add(newKey, value);
        }

        if (result.SortField is not null)
            result.SortField = result.SortField.Capitalize();

        return result;
    }

    public override void Write(Utf8JsonWriter writer, PrimeTableMetaData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}