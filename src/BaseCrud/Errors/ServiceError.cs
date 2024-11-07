using System.Text.Json;

namespace BaseCrud.Errors;

public record ServiceError(
    string ErrorMessage,
    string ErrorKey,
    string? Description = null)
{
    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; init; } = ErrorMessage;

    /// <summary>
    /// string key to use for internationalization
    /// </summary>
    public string ErrorKey { get; init; } = ErrorKey;

    /// <summary>
    /// Optional description about the error
    /// </summary>
    public string? Description { get; init; } = Description;

    public void Deconstruct(out string errorMessage, out string errorKey)
    {
        errorMessage = ErrorMessage;
        errorKey = ErrorKey;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}