using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization.Converters;

internal sealed class E5EHttpHeadersConverter : JsonConverter<E5EHttpHeaders>
{
	public override E5EHttpHeaders Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject token");

		var res = new E5EHttpHeaders();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				return res;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected PropertyName token");

			var propName = reader.GetString();
			if (propName is null)
				throw new JsonException("PropertyName cannot be null");

			reader.Read();
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Only string properties are supported");

			var value = reader.GetString();
			res.Add(propName, value);
		}

		throw new JsonException("Expected EndObject token");
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, E5EHttpHeaders value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var (key, values) in value)
			writer.WriteString(key, string.Join(", ", values));

		writer.WriteEndObject();
	}
}
