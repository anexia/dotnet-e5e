using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

public record E5ERequest
{
	[JsonPropertyName("data")] public JsonNode? Data { get; init; }

	[JsonPropertyName("request_headers")] public HttpRequestHeaders? RequestHeaders { get; init; }

	[JsonPropertyName("type")] public E5ERequestType Type { get; init; }

	[JsonPropertyName("params")] public E5ERequestParameters? Params { get; init; }


	public string? ReadAsString() => Data.Deserialize<string>();
	public TValue? Deserialize<TValue>(JsonSerializerOptions? options = null) => Data.Deserialize<TValue>(options);

	// TODO: how to deal with multipart/form-data? Via the MultipartFormDataContent class?
	public static E5ERequest FromString(string s)
	{
		return new E5ERequest { Type = E5ERequestType.Text, Data = JsonSerializer.SerializeToNode(s) };
	}
}
