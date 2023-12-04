namespace Anexia.E5E.Functions;

/// <summary>
///     Determines the type of the <see cref="E5EResponse.Data" />.
/// </summary>
public enum E5EResponseType
{
	/// <summary>
	///     The value is expected to be a string that is returned in the response body.
	/// </summary>
	Text,

	/// <summary>
	///     The value is expected to be a binary object representation that is returned in the response body.
	/// </summary>
	Binary,

	/// <summary>
	///     Primitive data types such as string, integer, decimal, boolean or null-type as well as structured data such as
	///     lists or dictionaries. Basically anything that can be represented as JSON structure in your runtime of choice.
	/// </summary>
	StructuredObject,
}
