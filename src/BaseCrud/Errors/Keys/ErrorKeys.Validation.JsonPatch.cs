namespace BaseCrud.Errors.Keys;

public static partial class ErrorKeys
{
    public static partial class Validation
    {
        public static class JsonPatch
        {
            private const string JsonPatchPrefix = Prefix + "jsonPatch.";

            public const string IdImmutable = JsonPatchPrefix + "idImmutable";

            public const string UnsupportedOperation = JsonPatchPrefix + "unsupportedOperation";

            public const string ApplyFailed = JsonPatchPrefix + "applyFailed";
        }
    }
}
