using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
///     The response for a given execution, serialized back as JSON.
/// </summary>
public class E5EResponse
{
	/// <summary>
	///     The response data that's serialized back.
	/// </summary>
	public JsonElement Data { get; init; }

	/// <summary>
	///     The type of the <see cref="Data" />.
	/// </summary>
	public E5EResponseType Type { get; init; }

	/// <summary>
	///     The HTTP status code that should be returned. Optional.
	/// </summary>
	public HttpStatusCode? Status { get; init; }

	/// <summary>
	///     A list of HTTP headers that should be attached to the response. Optional.
	/// </summary>
	public E5EHttpHeaders? ResponseHeaders { get; init; }

	/// <summary>
	///     Creates a new <see cref="E5EResponse" /> from the given text with the type
	///     <see cref="E5EResponseType.Text" />.
	/// </summary>
	/// <param name="text">The text to encode.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse" /> with the given data.</returns>
	public static E5EResponse From(string text, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null)
	{
		return new E5EResponse
		{
			Type = E5EResponseType.Text,
			Data = JsonSerializer.SerializeToElement(text, E5ESerializationContext.Default.String),
			Status = status,
			ResponseHeaders = responseHeaders,
		};
	}

	/// <summary>
	///     Creates a new <see cref="E5EResponse" /> from the given object with the type
	///     <see cref="E5EResponseType.StructuredObject" />.
	/// </summary>
	/// <param name="data">The data object.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse" /> with the given data.</returns>
	[RequiresUnreferencedCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#if NET8_0_OR_GREATER
	[RequiresDynamicCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#endif
	public static E5EResponse From<T>(T data, HttpStatusCode? status = null, E5EHttpHeaders? responseHeaders = null)
		where T : notnull
	{
		return new E5EResponse
		{
			Type = E5EResponseType.StructuredObject,
			Data = JsonSerializer.SerializeToElement(data),
			Status = status,
			ResponseHeaders = responseHeaders,
		};
	}

	/// <summary>
	///     Creates a new <see cref="E5EResponse" /> from the given file with the type
	///     <see cref="E5EResponseType.Binary" />.
	/// </summary>
	/// <param name="file">The file contents.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse" /> with the given data.</returns>

#if !NET8_0_OR_GREATER
	[RequiresUnreferencedCode(
		"This helper relies on runtime reflection for the JSON serialization.")]
#endif
	public static E5EResponse From(E5EFileData file, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null)
	{
		return new E5EResponse
		{
			Type = E5EResponseType.Binary,
#if NET8_0_OR_GREATER
			Data = JsonSerializer.SerializeToElement(file, E5ESerializationContext.Default.E5EFileData),
#else
			Data = JsonSerializer.SerializeToElement(file, E5EJsonSerializerOptions.Default),
#endif
			Status = status,
			ResponseHeaders = responseHeaders,
		};
	}

	/// <summary>
	///     Creates a new <see cref="E5EResponse" /> from the given object with the type
	///     <see cref="E5EResponseType.Binary" />.
	/// </summary>
	/// <remarks>For further control about the information that is sent to the client (filename, charset, etc.), it's recommended to use the <see cref="E5EFileData"/> instead.</remarks>
	/// <param name="data">The raw binary contents.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse" /> with the given data.</returns>
	[RequiresUnreferencedCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#if NET8_0_OR_GREATER
	[RequiresDynamicCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#endif
	public static E5EResponse From(IEnumerable<byte> data, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null)
	{
		return From(data.ToArray());
	}

	/// <summary>
	///     Creates a new <see cref="E5EResponse" /> from the given object with the type
	///     <see cref="E5EResponseType.Binary" />.
	/// </summary>
	/// <remarks>For further control about the information that is sent to the client (filename, charset, etc.), it's recommended to use the <see cref="E5EFileData"/> instead.</remarks>
	/// <param name="data">The raw binary contents.</param>
	/// <param name="status">An optional HTTP status code.</param>
	/// <param name="responseHeaders">An optional list of HTTP headers.</param>
	/// <returns>A valid <see cref="E5EResponse" /> with the given data.</returns>
	[RequiresUnreferencedCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#if NET8_0_OR_GREATER
	[RequiresDynamicCode(
		"This helper relies on runtime reflection for the JSON serialization. Initialize the E5EResponse by yourself for AOT.")]
#endif
	public static E5EResponse From(byte[] data, HttpStatusCode? status = null,
		E5EHttpHeaders? responseHeaders = null)
	{
		return From(
			new E5EFileData(data)
			{
				ContentType = "application/octet-stream",
				Filename = "dotnet-e5e-binary-response.blob",
			}, status, responseHeaders);
	}
}
