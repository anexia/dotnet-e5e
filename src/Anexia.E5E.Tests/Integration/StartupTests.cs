using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions.Termination;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.TestHelpers;

using Microsoft.Extensions.DependencyInjection;

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
		var expected = JsonSerializer.Serialize(E5ERuntimeMetadata.Current, E5EJsonSerializerOptions.Default);
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
		var terminator = Host.Inner.Services.GetRequiredService<ITerminator>() as TerminatorMock;
		Assert.True(terminator?.Called);
	}
}
