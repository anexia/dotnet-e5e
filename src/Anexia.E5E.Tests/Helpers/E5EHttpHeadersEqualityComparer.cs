using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Tests.Helpers;

public sealed class E5EHttpHeadersEqualityComparer : IEqualityComparer<E5EHttpHeaders?>
{
	public static readonly E5EHttpHeadersEqualityComparer Instance = new();

	public bool Equals(E5EHttpHeaders? x, E5EHttpHeaders? y)
	{
		if (x is null || y is null) return false;

		foreach (var (k1, v1) in x)
		{
			var enumerable = v1.ToList();
			foreach (var (k2, v2) in y)
			{
				if (k1 != k2)
					return false;

				if (!enumerable.SequenceEqual(v2)) return false;
			}
		}

		return true;
	}

	public int GetHashCode(E5EHttpHeaders obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
