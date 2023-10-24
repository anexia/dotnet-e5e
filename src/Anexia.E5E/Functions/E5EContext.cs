using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

public record E5EContext
{
	public E5EContext(E5EContextType type, DateTimeOffset date, bool isAsynchronous)
	{
		Type = type;
		Date = date;
		IsAsynchronous = isAsynchronous;
	}

	[JsonPropertyName("type")] public E5EContextType Type { get; init; } = E5EContextType.Generic;
	[JsonPropertyName("date")] public DateTimeOffset Date { get; init; }
	[JsonPropertyName("async")] public bool IsAsynchronous { get; init; }
}
