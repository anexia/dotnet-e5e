using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization.Converters;

namespace Anexia.E5E.Serialization;

[JsonSerializable(typeof(string))]
#if NET8_0_OR_GREATER
// The .NET 6 JSON generation is very limited, especially with the lack of init-only properties.
// We rely on reflection for our types in .NET 6 and use the AOT-compatible code generation beginning with .NET 8.
[JsonSerializable(typeof(E5ERequest))]
[JsonSerializable(typeof(E5EResponse))]
[JsonSerializable(typeof(E5ERuntimeMetadata))]
[JsonSerializable(typeof(E5EFileData))]
[JsonSerializable(typeof(IEnumerable<E5EFileData>))]
[JsonSourceGenerationOptions(
	JsonSerializerDefaults.General,
	IgnoreReadOnlyProperties = false,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
	Converters = new []
	{
		typeof(E5EHttpHeadersConverter),
		typeof(E5EResponseTypeConverter),
		typeof(E5ERequestDataTypeConverter),
	}
)]
#endif
internal sealed partial class E5ESerializationContext : JsonSerializerContext
{
}
