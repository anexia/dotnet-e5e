namespace Anexia.E5E.Exceptions;

public abstract class E5EException : Exception
{
	protected E5EException() { }


	protected E5EException(string message) : base(message)
	{
	}

	protected E5EException(Exception innerException) : base("", innerException) { }

	protected E5EException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
