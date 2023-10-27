using System.Globalization;
using System.Text.Json;

namespace Anexia.E5E.Serialization;

internal class JsonLowerSnakeCasePolicy : JsonNamingPolicy
{
	public override string ConvertName(string name)
	{
		return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
			.ToLower(CultureInfo.InvariantCulture);
	}
}
