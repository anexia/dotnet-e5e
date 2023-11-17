using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Anexia.E5E.Functions;

/// <summary>
///     Contains HTTP headers, normalized according to RFC 2616.
/// </summary>
/// <remarks>
///     Right now, E5E only allows a header to exist only one time, multiple values per key are not implemented.
///     Therefore, in order to maintain compatibility, multiple values with the same key are comma-separated.
/// </remarks>
public class E5EHttpHeaders : HttpHeaders
{
	/// <summary>
	///     Return if a specified header and specified value is stored in the collection.
	/// </summary>
	/// <param name="name">The specified header.</param>
	/// <param name="header">The specified value.</param>
	/// <returns>true if the specified header name and values are stored in the collection; otherwise false.</returns>
	[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
	public bool TryGetValue(string name, out string? header)
	{
		if (!TryGetValues(name, out var headers))
		{
			header = null;
			return false;
		}

		header = string.Join(", ", headers);
		return true;
	}
}
