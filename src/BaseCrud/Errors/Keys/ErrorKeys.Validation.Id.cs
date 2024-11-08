namespace BaseCrud.Errors.Keys;

public static partial class ErrorKeys
{
    public static partial class Validation
    {
        public static class Id
        {
            private const string IdPrefix = Prefix + "id.";

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public const string Error = IdPrefix + "error";

            public const string EmptyGuid = IdPrefix + "empty-guid";
            
            public const string ShouldBeZero = IdPrefix + "not-zero";

            public const string ShouldNotBeZero = IdPrefix + "is-zero";

            public const string ShouldBeDefault = IdPrefix + "invalid-id-should-be-default";

            public const string ShouldNotBeDefault = IdPrefix + "invalid-id-should-not-be-default";
        }
    }
}