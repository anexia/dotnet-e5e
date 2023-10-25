namespace Anexia.E5E.Exceptions;

public class E5EFailedDeserializationException : E5EException
{
	public override string Message => "The JSON deserialization of the given line failed.";

	public string Line { get; } = string.Empty;

	public E5EFailedDeserializationException(string line, Exception innerException) : base("", innerException)
	{
		Line = line;
	}

	public E5EFailedDeserializationException(string line)
	{
		Line = line;
	}
}
