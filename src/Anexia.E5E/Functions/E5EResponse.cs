using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

// TODO: Implement .NET 6 compatible polymorphic deserialization. It's supported natively in .NET 7, but let's target
// the LTS release here. https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#support-polymorphic-deserialization
public class E5EResponse
{
	[JsonInclude] public JsonNode? Data { get; }

	public E5EResponseType Type { get; private set; }

	public HttpStatusCode? Status { get; protected set; }
	public HttpResponseHeaders? ResponseHeaders { get; protected set; }

	[JsonConstructor]
	public E5EResponse()
	{
	}

	internal E5EResponse(JsonNode? node) => Data = node;

	public static E5EResponse FromString(string? s)
	{
		return new E5EResponse(JsonSerializer.SerializeToNode(s)) { Type = E5EResponseType.Text, };
	}

	public static E5EResponse From<T>(T? obj, JsonSerializerOptions? options = null)
	{
		return new E5EResponse(JsonSerializer.SerializeToNode(obj, options)) { Type = E5EResponseType.Object, };
	}

	public static E5EResponse FromBinary(byte[] data)
	{
		return new E5EResponse(JsonSerializer.SerializeToNode(data)) { Type = E5EResponseType.Binary, };
	}
}
