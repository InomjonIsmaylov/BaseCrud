using System.Diagnostics.CodeAnalysis;

namespace BaseCrud.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// returns the string by changing the first letter to capital
    /// </summary>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Capitalize(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result1 = string.Create(value.Length, value, (result, original) =>
        {
            ReadOnlySpan<char> spans = value.AsSpan();
            result[0] = char.ToUpper(spans[0]);

            for (var i = 1; i < original.Length; i++)
            {
                result[i] = spans[i];
            }
        });

        return result1;
    }
}