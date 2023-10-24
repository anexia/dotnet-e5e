namespace Anexia.E5E.Exceptions;

public class E5EFailedDeserializationException : E5EException
{
	public E5EFailedDeserializationException(string message, Exception innerException) : base(message, innerException)
	{
	}

	public E5EFailedDeserializationException(string message) : base(message)
	{
	}
}
