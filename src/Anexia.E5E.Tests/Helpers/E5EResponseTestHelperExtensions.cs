using System.Text.Json;

using Anexia.E5E.Functions;

using Xunit;

namespace Anexia.E5E.Tests.Helpers;

public static class E5EResponseTestHelperExtensions
{
	public static string Text(this E5EResponse response)
	{
		Assert.Equal(E5EResponseType.Text, response.Type);
		return response.Data.GetString()!;
	}

	public static T As<T>(this E5EResponse response)
	{
		Assert.Equal(E5EResponseType.StructuredObject, response.Type);
		var res = response.Data.Deserialize<T>();
		Assert.NotNull(res);
		return res;
	}
}
