using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Anexia.E5E.Functions;

/// <summary>
/// Contains HTTP headers, normalized according to RFC 2616.
/// </summary>
/// <remarks>
/// Right now, E5E only allows a header to exist only one time, multiple values per key are not implemented.
/// Therefore, in order to maintain compatibility, multiple values with the same key are comma-separated.
/// </remarks>
public class E5EHttpHeaders : HttpHeaders
{
	/// <summary>
	/// Return if a specified header and specified value is stored in the collection.
	/// </summary>
	/// <param name="name">The specified header.</param>
	/// <param name="header">The specified value.</param>
	/// <returns>true if the specified header name and values are stored in the collection; otherwise false.</returns>
	public bool TryGetValue(string name, out string? header)
	{
		if (!this.TryGetValues(name, out var headers))
		{
			header = null;
			return false;
		}

		header = string.Join(", ", headers);
		return true;
	}

	private sealed class E5EHttpHeadersEqualityComparer : IEqualityComparer<E5EHttpHeaders>
	{
		public bool Equals(E5EHttpHeaders? x, E5EHttpHeaders? y)
		{
			if (x is null || y is null) return false;

			foreach ((string? k1, IEnumerable<string>? v1) in x)
			{
				var enumerable = v1.ToList();
				foreach ((string? k2, IEnumerable<string>? v2) in y)
				{
					if (k1 != k2)
						return false;

					if (!enumerable.SequenceEqual(v2)) return false;
				}
			}

			return true;
		}

		public int GetHashCode(E5EHttpHeaders obj) => RuntimeHelpers.GetHashCode(obj);
	}

	public static IEqualityComparer<E5EHttpHeaders> E5EHttpHeadersComparer { get; } =
		new E5EHttpHeadersEqualityComparer();
}
