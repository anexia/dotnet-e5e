using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public class NonKeepAliveModeTests : IntegrationTestBase
{
	public NonKeepAliveModeTests(ITestOutputHelper outputHelper) : base(outputHelper)
	{
		Host.ConfigureEndpoints(cfg =>
		{
			cfg.RegisterEntrypoint(TestE5ERuntimeOptions.DefaultEntrypointName,
				_ => Task.FromResult(E5EResponse.From("response")));
		}, new[] { TestE5ERuntimeOptions.DefaultEntrypointName, "+++", "0", "---" });
	}

	public override Task InitializeAsync() => Host.StartAsync();


	[Fact]
	public async Task ShouldEndWithJsonResponse()
	{
		var (stdout, _) = await Host.WriteOnceAsync(builder => builder.WithData("request"));
		Assert.EndsWith(@"{""result"":{""data"":""response"",""type"":""text""}}", stdout);
	}

	[Fact]
	public async Task ShouldNotWriteExecutionEndToStderr()
	{
		var (_, stderr) = await Host.WriteOnceAsync(builder => builder.WithData("request"));
		Assert.Empty(stderr);
	}
}
