using System.Text.Json.Serialization;

namespace Anexia.E5E.Functions;

/// <summary>
/// Contains information about the files that were attached to this request.
/// </summary>
/// <remarks>For requests, it requires that it has the type of <see cref="E5ERequestDataType.Binary"/> or <see cref="E5ERequestDataType.Mixed"/>.</remarks>
public sealed record E5EFileData
{
	/// <summary>
	/// The contents of the file, encoded in the charset given by <see cref="Charset"/>.
	/// </summary>
	[JsonPropertyName("binary")]
	public byte[] Bytes { get; } = Array.Empty<byte>();

	/// <summary>
	/// The type of this binary, usually just <code>binary</code>.
	/// </summary>
	[JsonPropertyName("type")]
	public string Type { get; init; } = "binary";

	/// <summary>
	/// The size of the file in bytes. Can be zero if it cannot be determined reliably.
	/// </summary>
	[JsonPropertyName("size")]
	public long FileSizeInBytes { get; init; }

	/// <summary>
	/// The optional filename of the file.
	/// </summary>
	[JsonPropertyName("name")]
	public string? Filename { get; init; }

	/// <summary>
	/// The content type of the file.
	/// </summary>
	/// <remarks>
	/// For responses, the <code>Content-Type</code> header is set automatically by the E5E engine to this value.
	/// </remarks>
	[JsonPropertyName("content_type")]
	public string? ContentType { get; init; }

	/// <summary>
	/// The charset of the file. Recommended and also the default value is "utf-8".
	/// </summary>
	[JsonPropertyName("charset")]
	public string Charset { get; init; } = "utf-8";

	/// <summary>
	/// Creates a new file from the given bytes with "utf-8" as the charset.
	/// </summary>
	/// <param name="bytes">The contents of the file.</param>
	public E5EFileData(byte[] bytes)
	{
		Charset = "utf-8";
		Bytes = bytes;
	}
}
