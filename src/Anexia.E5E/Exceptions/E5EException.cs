namespace Anexia.E5E.Exceptions;

public class E5EException : Exception
{
	public E5EException(string message, Exception innerException) : base(message, innerException)
	{
	}

	public E5EException(string message) : base(message)
	{
	}
}
