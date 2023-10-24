using System.Text.Json.Serialization;

namespace Anexia.E5E.Serialization;

public class E5EEnumJsonConverter : JsonStringEnumConverter
{
	public E5EEnumJsonConverter() : base(new JsonLowerSnakeCasePolicy())
	{
	}
}
