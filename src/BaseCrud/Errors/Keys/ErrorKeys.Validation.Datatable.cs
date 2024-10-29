namespace BaseCrud.Errors.Keys;

public static partial class ErrorKeys
{
    public static partial class Validation
    {
        public static class Datatable
        {
            private const string DtPrefix = Prefix + "datatable.";

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public const string Error = DtPrefix + "error";

            public const string RowsCountMustBeGreaterThanZero = DtPrefix + "rows-number-below-one";

            public const string FirstMustBeGreaterThanOrEqualToZero = DtPrefix + "first-number-below-one";
        }
    }
}