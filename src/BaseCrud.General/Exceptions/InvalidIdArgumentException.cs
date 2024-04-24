using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BaseCrud.General.Exceptions;

[Serializable]
public class InvalidIdArgumentException : ValidationException
{
    public InvalidIdArgumentException() : base(GetMessage()) { }

    public InvalidIdArgumentException(string message) : base(GetMessage(message)) { }

    public InvalidIdArgumentException(string message, Exception inner) : base(GetMessage(message), inner) { }

    public static void ThrowIfInvalid(object argument, string? argumentName = "")
    {
        if (argument is null)
            throw new InvalidIdArgumentException($"Argument {argumentName ?? "id"} can not be null");

        switch (argument)
        {
            case int i:
                ThrowIfZero(i, argumentName);
                break;
            case Guid g:
                CheckId(g, argumentName);
                break;
            case string s:
                CheckId(s, argumentName);
                break;
            default:
                throw new InvalidIdArgumentException($"Argument {argumentName ?? "id"} is not a valid type");
        }
    }

    public static void ThrowIfZero(int argument, string? argumentName = null)
    {
        if (argument == 0)
            throw new InvalidIdArgumentException($"Argument {argumentName ?? "id"} can not be zero");
    }

    public static void CheckId(Guid id, string? argumentName = null)
    {
        if (id == Guid.Empty)
            throw new InvalidIdArgumentException($"Argument {argumentName ?? "id"} can not be an empty String");
    }

    public static void CheckId([NotNull] string? id, string? idArgumentName = null)
    {
        switch (id)
        {
            case null:
                throw new InvalidIdArgumentException($"{idArgumentName ?? "id"} argument is null");
            case "":
                throw new InvalidIdArgumentException($"{idArgumentName ?? "id"} argument is empty string");
        }

        if (id == Guid.Empty.ToString())
            throw new InvalidIdArgumentException($"{idArgumentName ?? "id"} argument is empty guid");
    }

    public static void CheckIdGuidString([NotNull] string? id, string idArgumentName = "id")
    {
        CheckId(id, idArgumentName);

        if (!Guid.TryParse(id, out _))
            throw new InvalidIdArgumentException($"{idArgumentName} argument is not guid string");
    }

    private static string GetMessage(string? message = null)
    {
        const string msg = "Invalid Id argument error";

        if (message != null)
            return msg + ": " + message;

        return msg;
    }
}