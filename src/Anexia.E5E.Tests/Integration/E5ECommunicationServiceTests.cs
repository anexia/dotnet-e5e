using System;
using System.Linq;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public class E5ECommunicationServiceTests : IntegrationTestBase
{
	public E5ECommunicationServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
	{
	}

	[Fact]
	public async Task PingResponds()
	{
		await Host.StartWithTestEntrypointAsync(_ => throw new Exception("This method should not have been called"));
		var (stdout, stderr) = await Host.WriteOnceAsync("ping");
		Assert.Equal("pong---", stdout);
		Assert.Equal("---", stderr);
	}

	[Fact]
	public async Task PlainInput()
	{
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From("test"));
		var resp = await Host.WriteExampleRequestOnceAsync();
		Assert.Equal("test", resp.Text());
	}

	[Fact]
	public async Task HeadersAreReceived()
	{
		const string headerName = "Accept";
		await Host.StartWithTestEntrypointAsync(req =>
		{
			Assert.NotNull(req.Event.RequestHeaders);

			req.Event.RequestHeaders.TryGetValue(headerName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!);
		});
		var resp = await Host.WriteRequestOnceAsync(req => req.AddHeader(headerName, "application/json"));
		Assert.Equal("application/json", resp.Text());
	}

	[Fact]
	public async Task EventDataIsReceived()
	{
		await Host.StartWithTestEntrypointAsync(req =>
		{
			Assert.Equal("data", req.Event.Data.ToString());
			return E5EResponse.From("success");
		});

		var resp = await Host.WriteRequestOnceAsync(req => req.WithData("data"));
		Assert.Equal("success", resp.Text());
	}

	[Fact]
	public async Task ParamsAreReceived()
	{
		const string parameterName = "myParam";
		await Host.StartWithTestEntrypointAsync(req =>
		{
			Assert.NotNull(req.Event.Params);

			req.Event.Params.TryGetValue(parameterName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!.First());
		});

		var resp = await Host.WriteRequestOnceAsync(req => req.AddParam(parameterName, "param"));
		Assert.Equal("param", resp.Text());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From("response"));
		var (stdout, _) = await Host.WriteOnceAsync(builder => builder.WithData("request"));
		Assert.Equal(@"+++{""data"":""response"",""type"":""text""}---", stdout);
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormatting()
	{
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From("response"));
		var (_, stderr) = await Host.WriteOnceAsync(builder => builder.WithData("request"));
		Assert.Equal("---", stderr);
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormattingOnException()
	{
		await Host.StartWithTestEntrypointAsync(_ => throw new Exception("please fail uwu"));
		await Assert.ThrowsAsync<TaskCanceledException>(() => Host.WriteExampleRequestOnceAsync());

		var stderr = await Host.GetStderrAsync();
		Assert.Empty(stderr);
	}

	[Fact]
	public async Task ErrorSetsEnvironmentExitCode()
	{
		await Host.StartWithTestEntrypointAsync(_ => throw new Exception("please fail uwu"));
		await Assert.ThrowsAsync<TaskCanceledException>(() => Host.WriteExampleRequestOnceAsync());
		Assert.NotEqual(0, Environment.ExitCode);
	}
}
