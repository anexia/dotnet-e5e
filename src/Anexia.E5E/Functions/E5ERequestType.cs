using System.Text.Json.Serialization;

using Anexia.E5E.Serialization;

namespace Anexia.E5E.Functions;

[JsonConverter(typeof(E5EEnumJsonConverter))]
public enum E5ERequestType
{
	Text,
	Binary,
	Object,
	Mixed
}
