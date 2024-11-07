namespace BaseCrud.Errors.Keys;

public static partial class ErrorKeys
{
    public static class Database
    {
        private const string Prefix = "db.";

        public const string UpdateError = Prefix + "update-error";

        public const string InsertError = Prefix + "insert-error";

        public const string EntityDeactivated = Prefix + "is-deactivated";

        public const string NotFoundById = Prefix + "not-found-by-id";

        public const string EntitiesToChange = "no-entities-to-change";
    }
}