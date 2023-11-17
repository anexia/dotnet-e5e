using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
///     Provides information about the current execution.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record E5EContext
{
	/// <summary>
	///     Initializes a new instance of the <see cref="E5EContext" /> record.
	/// </summary>
	/// <param name="type">Used for differentiation of the execution context.</param>
	/// <param name="date">The invocation date of the function.</param>
	/// <param name="isAsynchronous">Whether the call is made asynchronous or not.</param>
	/// <param name="data">Additional optional data.</param>
	public E5EContext(string type, DateTimeOffset date, bool isAsynchronous, JsonElement? data = null)
	{
		Type = type;
		Date = date;
		IsAsynchronous = isAsynchronous;
		Data = data;
	}

	/// <summary>
	///     Contains additional data about the context in which the function was executed.
	///     For example, this attribute may contain the returned object of an authorizer function that was executed before the
	///     function.
	/// </summary>
	public JsonElement? Data { get; init; }

	/// <summary>
	///     Tells information about the trigger that caused this execution.
	/// </summary>
	public string Type { get; init; } = "generic";

	/// <summary>
	///     The time when the function was triggered.
	/// </summary>
	public DateTimeOffset Date { get; init; }

	/// <summary>
	///     This attribute is set to true if the event was triggered in an asynchronous way, meaning that the event trigger
	///     does not wait for the return of the function execution.
	/// </summary>
	[JsonPropertyName("async")]
	public bool IsAsynchronous { get; init; }
}
