using System.Text.Json;
using System.Text.Json.Serialization;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization.Converters;

internal class E5EHttpHeadersConverter : JsonConverter<E5EHttpHeaders>
{
	public override E5EHttpHeaders Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var res = new E5EHttpHeaders();
		var node = JsonSerializer.Deserialize<JsonDocument>(ref reader);
		if (node is null)
			throw new JsonException("Reading JSON into JSON document failed");

		foreach (var prop in node.RootElement.EnumerateObject())
		{
			switch (prop.Value.ValueKind)
			{
				case JsonValueKind.String:
					res.Add(prop.Name, prop.Value.GetString());
					break;
				case JsonValueKind.Array:
					res.Add(prop.Name, prop.Value.EnumerateArray().Select(x => x.GetString()));
					break;
				default:
					throw new InvalidOperationException(
						$"Value of JSON property {prop.Name} is neither an array nor a string.");
			}
		}

		return res;
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, E5EHttpHeaders value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach ((string? key, IEnumerable<string>? values) in value)
			writer.WriteString(key, string.Join(", ", values));

		writer.WriteEndObject();
	}
}
