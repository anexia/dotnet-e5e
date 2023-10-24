using System.Text.Json;

namespace Anexia.E5E.Functions;

public record E5ERequest(E5ERequestType Type,
	JsonElement Data,
	E5EHttpHeaders? RequestHeaders = null,
	E5ERequestParameters? Params = null)
{
	public TValue? As<TValue>(JsonSerializerOptions? options = null) => Data.Deserialize<TValue>(options);
}
