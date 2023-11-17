namespace Anexia.E5E.Exceptions;

/// <summary>
///     The exception that is thrown when a response could not been serialized into JSON.
/// </summary>
public class E5EFailedSerializationException : E5EException
{
	internal E5EFailedSerializationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
