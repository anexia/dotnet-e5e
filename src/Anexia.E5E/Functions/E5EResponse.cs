using System.Net;
using System.Text.Json;

namespace Anexia.E5E.Functions;

/// <summary>
/// The response for a given execution, serialized back as JSON.
/// </summary>
public class E5EResponse
{
	private E5EResponse() { }

	/// <summary>
	/// The response data that's serialized back.
	/// </summary>
	public JsonElement Data { get; init; }

	/// <summary>
	/// The type of the <see cref="Data"/>.
	/// </summary>
	public E5EResponseType Type { get; init; }

	/// <summary>
	/// The HTTP status code that should be returned. Optional.
	/// </summary>
	public HttpStatusCode? Status { get; init; }

	/// <summary>
	/// A list of HTTP headers that should be attached to the response. Optional.
	/// </summary>
	public E5EHttpHeaders? ResponseHeaders { get; init; }

	/// <summary>
	/// Creates a new <see cref="E5EResponse"/> from the given object with the type <see cref="E5EResponseType.StructuredObject"/>.
	/// </summary>
	/// <param name="cls">The data object.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse"/> with the given data.</returns>
	public static E5EResponse From<T>(T cls, HttpStatusCode? status = null, E5EHttpHeaders? responseHeaders = null)
		where T : class => new()
		{
			Type = cls switch
			{
				IEnumerable<char> => E5EResponseType.Text,
				IEnumerable<byte> => E5EResponseType.Binary,
				_ => E5EResponseType.StructuredObject
			},
			Data = JsonSerializer.SerializeToElement(cls),
			Status = status,
			ResponseHeaders = responseHeaders
		};
}
