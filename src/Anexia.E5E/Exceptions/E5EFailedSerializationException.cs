namespace Anexia.E5E.Exceptions;

public class E5EFailedSerializationException : E5EException
{
	public E5EFailedSerializationException(string message, Exception innerException) : base(message, innerException)
	{
	}

	public E5EFailedSerializationException(string message) : base(message)
	{
	}
}
