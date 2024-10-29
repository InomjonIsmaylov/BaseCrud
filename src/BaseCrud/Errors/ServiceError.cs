namespace BaseCrud.Errors;

public record ServiceError(
    string ErrorMessage,
    string ErrorKey)
{
    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; init; } = ErrorMessage;

    /// <summary>
    /// string key to use for internationalization
    /// </summary>
    public string ErrorKey { get; init; } = ErrorKey;

    public void Deconstruct(out string errorMessage, out string errorKey)
    {
        errorMessage = ErrorMessage;
        errorKey = ErrorKey;
    }
}