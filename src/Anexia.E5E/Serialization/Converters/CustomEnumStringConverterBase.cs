using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization.Converters;

abstract class CustomEnumStringConverterBase<T> : JsonConverter<T>
{
	/// <summary>Determines whether the specified type can be converted.</summary>
	/// <param name="typeToConvert">The type to compare against.</param>
	/// <returns>
	/// <see langword="true" /> if the type can be converted; otherwise, <see langword="false" />.</returns>
	public override bool CanConvert(Type typeToConvert) => typeof(T) == typeToConvert;

	protected readonly Dictionary<string, T> _mapping = new();

	/// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString() ?? throw new JsonException(
			$"Input is not a string, cannot convert to {nameof(E5ERequestDataType)}");

		if (!_mapping.TryGetValue(str, out var res))
			throw new E5ERuntimeException($"The string {str} is not a known {typeof(T)}");

		return res;
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		(string key, T? _) = _mapping.FirstOrDefault(x => Equals(x.Value, value));
		if (key is null)
			throw new E5ERuntimeException($"The enum ${value} has no known serialization value.");

		writer.WriteStringValue(key);
	}
}
