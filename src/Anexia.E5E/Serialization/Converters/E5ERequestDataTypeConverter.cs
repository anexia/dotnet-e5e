using Anexia.E5E.Functions;

namespace Anexia.E5E.Serialization.Converters;

internal sealed class E5ERequestDataTypeConverter : CustomEnumStringConverterBase<E5ERequestDataType>
{
	public E5ERequestDataTypeConverter()
	{
		_mapping.Add("object", E5ERequestDataType.StructuredObject);
		_mapping.Add("mixed", E5ERequestDataType.Mixed);
		_mapping.Add("binary", E5ERequestDataType.Binary);
		_mapping.Add("text", E5ERequestDataType.Text);
	}
}
