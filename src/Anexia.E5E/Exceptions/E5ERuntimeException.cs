namespace Anexia.E5E.Exceptions;

/// <summary>
/// A generic exception for all errors that occur on runtime and might indicate erroneous behaviour.
/// </summary>
public sealed class E5ERuntimeException : E5EException
{
	internal E5ERuntimeException(string message) : base(message)
	{
		this.HelpLink = "https://github.com/anexia/dotnet-e5e/issues/new";
	}
}
