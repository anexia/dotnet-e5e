using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization.Converters;

internal sealed class E5EResponseTypeConverter : CustomEnumStringConverterBase<E5EResponseType>
{
	public E5EResponseTypeConverter()
	{
		_mapping.Add("object", E5EResponseType.StructuredObject);
		_mapping.Add("binary", E5EResponseType.Binary);
		_mapping.Add("text", E5EResponseType.Text);
	}
}
