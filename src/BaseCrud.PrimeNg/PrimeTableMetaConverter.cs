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

        result.Value = jsonElement switch
        {
            { ValueKind: JsonValueKind.Null } or { ValueKind: JsonValueKind.Undefined } => null,
            { ValueKind: JsonValueKind.False } => false,
            { ValueKind: JsonValueKind.True } => true,
            { ValueKind: JsonValueKind.Number } when int.TryParse(jsonElement.GetString(), out int resultInt) => resultInt,
            { ValueKind: JsonValueKind.Number } => reader.GetDouble(),
            { ValueKind: JsonValueKind.String } when reader.TryGetDateTime(out DateTime datetime) => datetime,
            { ValueKind: JsonValueKind.String } => reader.GetString()!,
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
    public override PrimeTableMetaData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = JsonSerializer.Deserialize<PrimeTableMetaData>(ref reader);

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

        return result;
    }

    public override void Write(Utf8JsonWriter writer, PrimeTableMetaData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}