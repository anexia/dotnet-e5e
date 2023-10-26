using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization;

internal class E5EEnumJsonConverter : JsonConverterFactory
{
	private readonly JsonStringEnumConverter _converter = new(new JsonLowerSnakeCasePolicy());

	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert == typeof(E5ERequestType) || typeToConvert == typeof(E5EResponseType);

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
		_converter.CreateConverter(typeToConvert, options);
}
