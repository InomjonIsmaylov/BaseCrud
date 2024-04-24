namespace BaseCrud.General.Exceptions;

[Serializable]
public class DatabaseOperationException : Exception
{
	public DatabaseOperationException() : base("Database operation exception") { }
	
	public DatabaseOperationException(string message) : base(message) { }
	
	public DatabaseOperationException(string message, Exception inner) : base(message, inner) { }
}
