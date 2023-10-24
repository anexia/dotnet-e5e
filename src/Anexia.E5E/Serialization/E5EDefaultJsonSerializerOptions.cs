using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Serialization;

public static class E5EJsonSerializerOptions
{
	public static JsonSerializerOptions Default
	{
		get
		{
			var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
			{
				PropertyNamingPolicy = new JsonLowerSnakeCasePolicy(),
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				IgnoreReadOnlyProperties = false,
			};
			options.Converters.Add(new E5EHttpHeadersConverter());
			return options;
		}
	}
}
