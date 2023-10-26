using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
/// The response for a given execution, serialized back as JSON.
/// </summary>
public class E5EResponse
{
	/// <summary>
	/// The response data that's serialized back.
	/// </summary>
	public JsonElement Data { get; protected set; }

	/// <summary>
	/// The type of the <see cref="Data"/>.
	/// </summary>
	public E5EResponseType Type { get; protected set; }

	/// <summary>
	/// The HTTP status code that should be returned. Optional.
	/// </summary>
	public HttpStatusCode? Status { get; protected set; }

	/// <summary>
	/// A list of HTTP headers that should be attached to the response. Optional.
	/// </summary>
	public E5EHttpHeaders? ResponseHeaders { get; protected set; }

	internal E5EResponse() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="E5EResponse"/>
	/// </summary>
	/// <param name="type"></param>
	/// <param name="data"></param>
	/// <param name="status"></param>
	/// <param name="responseHeaders"></param>
	[JsonConstructor]
	public E5EResponse(E5EResponseType type, JsonElement data, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null)
	{
		Type = type;
		Data = data;
		Status = status;
		ResponseHeaders = responseHeaders;
	}
}

/// <summary>
/// A typed version of the <seealso cref="E5EResponse{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the response.</typeparam>
public class E5EResponse<T> : E5EResponse
{
	/// <summary>
	/// The typed response value.
	/// </summary>
	public T Value { get; }

	[JsonConstructor]
	internal E5EResponse(E5EResponseType type, JsonElement data, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null) : base(type, data, status, responseHeaders)
	{
		Value = data.Deserialize<T>() ?? throw new InvalidOperationException(
			$"The provided data cannot be deserialized to {typeof(T)}");

		Type = Value switch
		{
			string => E5EResponseType.Text,
			IEnumerable<byte> => E5EResponseType.Binary,
			_ => E5EResponseType.Object
		};

		Data = data;
		Status = status;
		ResponseHeaders = responseHeaders;
	}

	/// <summary>
	/// Initializes a new typed response with the given value.
	/// </summary>
	/// <param name="value">The response value.</param>
	/// <param name="status">The HTTP status code. Optional.</param>
	/// <param name="responseHeaders">The HTTP response headers. Optional.</param>
	public E5EResponse(T value, HttpStatusCode? status = null, E5EHttpHeaders? responseHeaders = null)
	{
		Type = value switch
		{
			string => E5EResponseType.Text,
			IEnumerable<byte> => E5EResponseType.Binary,
			_ => E5EResponseType.Object
		};

		this.Value = value;
		Data = JsonSerializer.SerializeToElement(value);
		Status = status;
		ResponseHeaders = responseHeaders;
	}

	/// <summary>
	/// Implicitly converts a given response to its value.
	/// </summary>
	/// <param name="resp">The response.</param>
	/// <returns>The inner value.</returns>
	public static implicit operator T(E5EResponse<T> resp) => resp.Value;

	/// <summary>
	/// Converts any typed value to a proper response.
	/// </summary>
	/// <param name="input">The input.</param>
	/// <returns>The successful response.</returns>
	public static implicit operator E5EResponse<T>(T input) => new(input);
}
