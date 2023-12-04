using System.Diagnostics.CodeAnalysis;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     The exception that is thrown when the JSON deserialization of incoming messages failed.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EFailedDeserializationException : E5EException
{
	internal E5EFailedDeserializationException(string line, Exception innerException)
		: base("The JSON deserialization of the given line failed with an exception.", innerException)
	{
		Line = line;
	}

	/// <summary>
	///     The line that caused the serialization error.
	/// </summary>
	public string Line { get; }
}
