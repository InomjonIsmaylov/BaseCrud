using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseCrud.PrimeNg;

public class FilterMetadata
{
    /**
     * The value used for filtering.
     */
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /**
     * The match mode for filtering.
     */
    [JsonPropertyName("matchMode")]
    public string? MatchMode { get; set; }

    /**
     * The operator for filtering.
     */
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}