using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Serialization;

/// <summary>
/// Contains the default <see cref="JsonSerializerOptions"/> that are used internally.
/// </summary>
public static class E5EJsonSerializerOptions
{
	/// <summary>
	/// The e5e-specific serializer options:
	///   - The naming policy is using lower_snake_case for property names.
	///   - Null values are ignored.
	///   - Read-only properties are (de-)serialized as well.
	///   - Converters for <seealso cref="Functions.E5EHttpHeaders"/> and e5e-specific enums are added.
	/// </summary>
	public static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = new JsonLowerSnakeCasePolicy(),
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		IgnoreReadOnlyProperties = false,
		Converters = { new E5EHttpHeadersConverter(), new E5EEnumJsonConverter() }
	};
}
