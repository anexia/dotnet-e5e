namespace Anexia.E5E.Exceptions;

public class E5EMissingArgumentsException : E5EException
{
	public E5EMissingArgumentsException(string message, Exception innerException) : base(message, innerException)
	{
	}

	public E5EMissingArgumentsException(string message) : base(message)
	{
	}
}
