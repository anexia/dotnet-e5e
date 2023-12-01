using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Serialization.Converters;

namespace Anexia.E5E.Serialization;

/// <summary>
///     Contains the default <see cref="JsonSerializerOptions" /> that are used internally.
/// </summary>
public static class E5EJsonSerializerOptions
{
	/// <summary>
	///     The E5E-specific serializer options:
	///     - The naming policy is using lower_snake_case for property names.
	///     - Null values are ignored.
	///     - Read-only properties are (de-)serialized as well.
	///     - Converters for <seealso cref="Functions.E5EHttpHeaders" /> and E5E-specific enums are added.
	/// </summary>
	public static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.General)
	{
		// Be sure to sync this with the options in the E5ESerializationContext!
		IgnoreReadOnlyProperties = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters =
		{
			new E5EHttpHeadersConverter(), new E5EResponseTypeConverter(), new E5ERequestDataTypeConverter(),
		},
#if NET8_0_OR_GREATER
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
#else
		PropertyNamingPolicy = new JsonLowerSnakeCasePolicy(),
#endif
	};
}
