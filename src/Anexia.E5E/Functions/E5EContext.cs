using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

public record E5EContext
{
	public E5EContext(string type, DateTimeOffset date, bool isAsynchronous)
	{
		Type = type;
		Date = date;
		IsAsynchronous = isAsynchronous;
	}

	[JsonPropertyName("type")] public string Type { get; init; } = "generic";
	[JsonPropertyName("date")] public DateTimeOffset Date { get; init; }
	[JsonPropertyName("async")] public bool IsAsynchronous { get; init; }
}
