using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

/// <summary>
/// Thrown when a <see cref="E5EEvent" /> is converted into the wrong format.
/// </summary>
public class E5EInvalidConversionException : E5EException
{
	/// <summary>
	/// The required data type for this conversion call.
	/// </summary>
	public E5ERequestDataType Expected { get; }

	/// <summary>
	/// The actual data type.
	/// </summary>
	public E5ERequestDataType Actual { get; }

	internal E5EInvalidConversionException(E5ERequestDataType expected, E5ERequestDataType actual)
		: base($"Cannot convert data of type {actual} into the type {expected}")
	{
		Expected = expected;
		Actual = actual;
	}

	internal static void ThrowIfNotMatch(E5ERequestDataType expected, E5ERequestDataType actual)
	{
		if (expected != actual)
			throw new E5EInvalidConversionException(expected, actual);
	}
}
