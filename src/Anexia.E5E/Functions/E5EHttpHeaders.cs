using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Anexia.E5E.Functions;

public class E5EHttpHeaders : HttpHeaders
{
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
