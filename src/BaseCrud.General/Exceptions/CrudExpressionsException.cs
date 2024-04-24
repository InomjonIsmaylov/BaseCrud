namespace BaseCrud.General.Exceptions;

[Serializable]
public class CrudExpressionsException : Exception
{
	public CrudExpressionsException() { }
	
	public CrudExpressionsException(string message) : base(message) { }
	
	public CrudExpressionsException(string message, Exception inner) : base(message, inner) { }
}
