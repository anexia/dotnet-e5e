using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.TestHelpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public class StartupTests : IntegrationTestBase
{
	public StartupTests(ITestOutputHelper outputHelper) : base(outputHelper)
	{
		Host.ConfigureEndpoints(static _ => { }, new[] { "metadata" });
	}

	[Fact]
	public async Task OutputMatches()
	{
		await Host.StartAsync();
		var expected = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
		var stdout = await Host.GetStdoutAsync();
		Assert.Equal(expected, stdout);
	}

	[Fact]
	public async Task TerminatesAutomatically()
	{
		await Host.StartAsync();

		var cts = new CancellationTokenSource(3000);
		cts.Token.Register(() => Assert.Fail("Shutdown took longer than three seconds"), true);

		await Host.WaitForShutdownAsync(cts.Token);
	}
}
