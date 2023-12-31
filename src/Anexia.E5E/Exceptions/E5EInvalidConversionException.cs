using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     Thrown when a <see cref="E5EEvent" /> is converted into the wrong format.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EInvalidConversionException : E5EException
{
	private E5EInvalidConversionException(E5ERequestDataType expected, E5ERequestDataType actual)
		: base($"Cannot convert data of type {actual} into the type {expected}")
	{
		Expected = expected;
		Actual = actual;
	}

	/// <summary>
	///     The required data type for this conversion call.
	/// </summary>
	public E5ERequestDataType Expected { get; }

	/// <summary>
	///     The actual data type.
	/// </summary>
	public E5ERequestDataType Actual { get; }

	internal static void ThrowIfNotMatch(E5ERequestDataType expected, E5ERequestDataType actual)
	{
		if (expected != actual)
			throw new E5EInvalidConversionException(expected, actual);
	}
}
