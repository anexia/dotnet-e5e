using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
/// Provides information about the current execution.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record E5ERequestContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="E5ERequestContext"/> record.
	/// </summary>
	/// <param name="type">Used for differentiation of the execution context.</param>
	/// <param name="date">The invocation date of the function.</param>
	/// <param name="isAsynchronous">Whether the call is made asynchronous or not.</param>
	public E5ERequestContext(string type, DateTimeOffset date, bool isAsynchronous)
	{
		Type = type;
		Date = date;
		IsAsynchronous = isAsynchronous;
	}

	/// <summary>
	/// Tells information about the trigger that caused this execution.
	/// </summary>
	public string Type { get; init; } = "generic";

	/// <summary>
	/// The time when the function was triggered.
	/// </summary>
	public DateTimeOffset Date { get; init; }

	/// <summary>
	/// This attribute is set to true if the event was triggered in an asynchronous way, meaning that the event trigger
	/// does not wait for the return of the function execution.
	/// </summary>
	[JsonPropertyName("async")]
	public bool IsAsynchronous { get; init; }
}
