using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseCrud.PrimeNg;

public class FilterMetadata
{
    /**
     * The value used for filtering.
     */
    [JsonPropertyName("value")]
    [DefaultValue(1)]
    public object? Value { get; set; }

    /**
     * The match mode for filtering.
     */
    [JsonPropertyName("matchMode")]
    [DefaultValue("equals")]
    public string? MatchMode { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}