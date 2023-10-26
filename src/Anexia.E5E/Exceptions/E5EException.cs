namespace Anexia.E5E.Exceptions;

/// <summary>
/// A generic exception that all E5E-specific errors inherit from.
/// </summary>
public abstract class E5EException : Exception
{
	internal E5EException(string message) : base(message)
	{
	}

	internal E5EException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
