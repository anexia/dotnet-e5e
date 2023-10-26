namespace Anexia.E5E.Exceptions;

/// <summary>
/// The exception that is thrown when there are arguments missing for startup.
/// </summary>
public class E5EMissingArgumentsException : E5EException
{
	internal E5EMissingArgumentsException(string message) : base(message)
	{
	}
}
