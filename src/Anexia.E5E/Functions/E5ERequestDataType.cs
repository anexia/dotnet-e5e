namespace Anexia.E5E.Functions;

/// <summary>
///     Determines the type of the request data, determined by the "Content-Type" HTTP header.
/// </summary>
public enum E5ERequestDataType
{
	/// <summary>
	///     Simple text, equivalent to the <code>text/*</code> content type.
	/// </summary>
	Text,

	/// <summary>Any unknown content type will result in this type.</summary>
	/// <remarks>Keep in mind that E5E, as any FaaS platform, is not very efficient when it comes to dealing with binary data.</remarks>
	Binary,

	/// <summary>
	///     Primitive data types such as string, integer, decimal, boolean or null-type as well as structured data such as
	///     lists or dictionaries.
	///     Basically, the data attribute contains the object you would receive if you deserialized a JSON in your runtime of
	///     choice.
	/// </summary>
	StructuredObject,

	/// <summary>
	///     The <see cref="E5EEvent.Data" /> property contains a key/value pair object, where each key
	///     represents a field name submitted by the client. Since a field name may occur multiple times within one request,
	///     the values of a field are always given as a list. Each value might be of a primitive data type such as (nullable)
	///     string,
	///     integer, decimal, bool or it might be a binary object representation.
	/// </summary>
	/// <remarks>Used for the <code>multipart/form-data</code> content type.</remarks>
	Mixed,
}
