using System.Text.Json.Serialization;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Runtime;

/// <summary>
/// Contains all information about the current function execution.
/// </summary>
public record E5EIncomingRequest
{
	/// <summary>
	/// Contains the user-provided request information, e.g. HTTP headers, the payload and other information.
	/// </summary>
	[JsonPropertyName("event")] public E5ERequest? Event { get; init; }

	/// <summary>
	/// Contains E5E-provided metadata about the current execution.
	/// </summary>
	[JsonPropertyName("context")] public E5EContext? Context { get; init; }
}
