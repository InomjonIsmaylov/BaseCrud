using BaseCrud.Errors;
using BaseCrud.Errors.Keys;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace BaseCrud.EntityFrameworkCore.JsonPatch;

internal static class JsonPatchOperationGuards
{
    /// <summary>
    /// Returns true when a guard failed and <paramref name="error"/> is set.
    /// </summary>
    public static bool TryGetGuardError<TModel>(
        IList<Operation<TModel>> operations,
        out ValidationServiceError? error)
        where TModel : class
    {
        foreach (Operation<TModel> op in operations)
        {
            if (op.OperationType is OperationType.Move or OperationType.Copy)
            {
                error = new ValidationServiceError(
                    $"JSON Patch operation '{op.OperationType}' is not supported.",
                    ErrorKeys.Validation.JsonPatch.UnsupportedOperation);
                return true;
            }

            if (TargetsEntityId(op.path) || TargetsEntityId(op.from))
            {
                error = new ValidationServiceError(
                    "JSON Patch must not target the entity Id (/id).",
                    ErrorKeys.Validation.JsonPatch.IdImmutable);
                return true;
            }
        }

        error = null;
        return false;
    }

    /// <summary>
    /// True only for JSON Pointer <c>/id</c> (case-insensitive). Nested paths like <c>/address/id</c> are false.
    /// </summary>
    public static bool TargetsEntityId(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        ReadOnlySpan<char> span = path.AsSpan().TrimEnd('/');
        return span.Equals("/id", StringComparison.OrdinalIgnoreCase);
    }
}
