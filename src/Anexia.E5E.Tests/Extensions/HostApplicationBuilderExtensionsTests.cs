using System.Linq;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Runtime;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Anexia.E5E.Tests.Extensions;

public class HostApplicationBuilderExtensionsTests
{
	public class ArgumentParsing
	{
		[Theory]
		[InlineData]
		[InlineData("foo", "bar")]
		public void ErroneousShouldFail(params object[] args)
		{
			var converted = args.Select(x => x.ToString() ?? "").ToArray();
			Assert.Throws<E5EMissingArgumentsException>(() =>
			{
				using var _ = Host.CreateDefaultBuilder()
					.UseAnexiaE5E(converted, static _ => { })
					.Build();
			});
		}

		[Fact]
		public void EscapedNullBytesAreHandled()
		{
			using var host = Host.CreateDefaultBuilder()
				.UseAnexiaE5E(new[] { "entrypoint", "\\0", "1", "\\0" }, static _ => { })
				.Build();

			var got = host.Services.GetRequiredService<E5ERuntimeOptions>();
			var expected = new E5ERuntimeOptions("entrypoint", "\0", "\0", true);

			Assert.Equal(expected, got);
		}
	}
}
