using System.Text.Json.Serialization;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Runtime;

public class E5EIncomingRequest
{
	[JsonPropertyName("event")] public E5ERequest? Event { get; set; }
	[JsonPropertyName("context")] public E5EContext? Context { get; set; }
}
