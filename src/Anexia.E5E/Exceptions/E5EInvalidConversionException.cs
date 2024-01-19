using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     Thrown when a <see cref="E5EEvent" /> is converted into the wrong format.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EInvalidConversionException : E5EException
{
	private E5EInvalidConversionException(E5ERequestDataType actual, E5ERequestDataType[] allowedTypes)
		: base($"Cannot convert data of type {actual} into one of {actual}")
	{
		AllowedTypes = allowedTypes;
		Actual = actual;
#pragma warning disable CS0618 // Type or member is obsolete
		Expected = allowedTypes[0];
#pragma warning restore CS0618 // Type or member is obsolete
	}

	/// <summary>
	///     The required data type for this conversion call. Obsolete, got replaced with <see cref="AllowedTypes"/>.
	/// </summary>
	[Obsolete(
		"The library got support for multiple allowedTypes data types per conversion. Please migrate this call to AllowedTypes.")]
	public E5ERequestDataType Expected { get; }

	/// <summary>
	///     The actual data type.
	/// </summary>
	public E5ERequestDataType Actual { get; }

	/// <summary>
	/// The allowed data types for this conversion call.
	/// </summary>
	public E5ERequestDataType[] AllowedTypes { get; }

	internal static void ThrowIfNotMatch(E5ERequestDataType value, params E5ERequestDataType[] allowed)
	{
		if (!allowed.Contains(value))
			throw new E5EInvalidConversionException(value, allowed);
	}
}
