using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
///     Contains information about the request that was made to the E5E endpoint.
/// </summary>
/// <param name="Type">The data type.</param>
/// <param name="Data">The data, not processed.</param>
/// <param name="RequestHeaders">The request headers, if any.</param>
/// <param name="Params">The request parameters, if any.</param>
public record E5EEvent(E5ERequestDataType Type,
	JsonElement? Data = null,
	E5EHttpHeaders? RequestHeaders = null,
	E5ERequestParameters? Params = null)
{
	/// <summary>
	///     Deserializes the request data into a <typeparamref name="TValue" />.
	/// </summary>
	/// <param name="options">If provided, additional options are used for deserialization.</param>
	/// <typeparam name="TValue">The target type.</typeparam>
	/// <returns>A <typeparamref name="TValue" /> representation of the JSON.</returns>
	/// <exception cref="JsonException">If <typeparamref name="TValue" /> is not compatible with the JSON.</exception>
	[RequiresUnreferencedCode(
		$"If you want to use AOT with this library, it's recommended to decode the {nameof(Data)} property by yourself.")]
#if NET8_0_OR_GREATER
	[RequiresDynamicCode(
		$"If you want to use AOT with this library, it's recommended to decode the {nameof(Data)} property by yourself.")]
#endif
	public TValue? As<TValue>(JsonSerializerOptions? options = null)
	{
		return Data.GetValueOrDefault().Deserialize<TValue>(options);
	}

	/// <summary>
	///     Deserializes the request data into a <typeparamref name="TValue" />.
	/// </summary>
	/// <param name="typeInfo">Metadata about the type to convert.</param>
	/// <typeparam name="TValue">The target type.</typeparam>
	/// <returns>A <typeparamref name="TValue" /> representation of the JSON.</returns>
	/// <exception cref="JsonException">If <typeparamref name="TValue" /> is not compatible with the JSON.</exception>
	public TValue? As<TValue>(JsonTypeInfo<TValue> typeInfo)
	{
		return Data.GetValueOrDefault().Deserialize(typeInfo);
	}

	/// <summary>
	///     Returns the value as string.
	/// </summary>
	/// <exception cref="E5EInvalidConversionException">
	///     Thrown if <see cref="Type" /> is not
	///     <see cref="E5ERequestDataType.Text" />.
	/// </exception>
	public string? AsText()
	{
		E5EInvalidConversionException.ThrowIfNotMatch(E5ERequestDataType.Text, Type);
		return As(E5ESerializationContext.Default.String);
	}

	/// <summary>
	///     Returns the value as byte enumerable.
	/// </summary>
	/// <exception cref="E5EInvalidConversionException">
	///     Thrown if <see cref="Type" /> is not
	///     <see cref="E5ERequestDataType.Binary" />.
	/// </exception>
	public byte[]? AsBytes()
	{
		E5EInvalidConversionException.ThrowIfNotMatch(E5ERequestDataType.Binary, Type);
		return As(E5ESerializationContext.Default.ByteArray);
	}
}
