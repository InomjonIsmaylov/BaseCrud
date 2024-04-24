using System.ComponentModel.DataAnnotations;

namespace BaseCrud.General.Exceptions;

[Serializable]
public class TableMetaDataInvalidException : ValidationException
{
    public TableMetaDataInvalidException() : base("Invalid TableMetaData") { }

    public TableMetaDataInvalidException(string message)
        : base($"Invalid TableMetaData. {message}")
    { }

    public TableMetaDataInvalidException(string message, Exception inner)
        : base($"Invalid TableMetaData. {message}", inner) { }
}
