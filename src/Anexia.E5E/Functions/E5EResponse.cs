using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

public class E5EResponse
{
	public JsonElement Data { get; protected set; }
	public E5EResponseType Type { get; protected set; }
	public HttpStatusCode? Status { get; protected set; }
	public E5EHttpHeaders? ResponseHeaders { get; protected set; }

	protected E5EResponse() { }

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

public class E5EResponse<T> : E5EResponse
{
	public T Value { get; }

	[JsonConstructor]
	public E5EResponse(E5EResponseType type, JsonElement data, HttpStatusCode? status = null,
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

	public static implicit operator T(E5EResponse<T> resp) => resp.Value;
	public static implicit operator E5EResponse<T>(T input) => new(input);
}
