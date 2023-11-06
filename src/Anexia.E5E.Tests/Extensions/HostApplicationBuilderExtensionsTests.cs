using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.TestHelpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Extensions;

public class HostApplicationBuilderExtensionsTests
{
	public class ArgumentParsing
	{
		[Theory]
		[InlineData]
		[InlineData("foo", "bar")]
		public void ErrornousShouldFail(params object[] args)
		{
			var converted = args.Select(x => x.ToString() ?? "").ToArray();
			Assert.Throws<E5EMissingArgumentsException>(() =>
			{
				using var _ = Host.CreateDefaultBuilder()
					.UseAnexiaE5E(converted)
					.Build();
			});
		}

		[Fact]
		public void EscapedNullBytesAreHandled()
		{
			using var host = Host.CreateDefaultBuilder()
				.UseAnexiaE5E(new[] { "entrypoint", "\\0", "1", "\\0" })
				.Build();

			var got = host.Services.GetRequiredService<E5ERuntimeOptions>();
			var expected = new E5ERuntimeOptions("entrypoint", "\0", "\0", true);

			Assert.Equal(expected, got);
		}
	}

	public class MetadataOnStartup : IntegrationTestBase
	{
		protected override IHostBuilder ConfigureE5E(IHostBuilder builder) =>
			builder.UseAnexiaE5E(new[] { "metadata" });

		public MetadataOnStartup(ITestOutputHelper outputHelper) : base(outputHelper)
		{
		}

		[Fact]
		public async Task OutputMatches()
		{
			var expected = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
			string stdout = await Host.GetStdoutAsync();
			Assert.Equal(expected, stdout);
		}

		[Fact]
		public Task TerminatesAutomatically()
		{
			var cts = new CancellationTokenSource(3000);
			cts.Token.Register(() => Assert.Fail("Shutdown took longer than three seconds"), true);

			return Host.WaitForShutdownAsync(cts.Token);
		}
	}
}
