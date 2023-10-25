using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Builders;
using Anexia.E5E.Tests.Helpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Hosting;

public class E5ECommunicationServiceTests
{
	private readonly ITestOutputHelper _outputHelper;

	public E5ECommunicationServiceTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

	[Fact]
	public async Task PingResponds()
	{
		// Arrange
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithFunction(TestE5ERuntimeOptions.DefaultEntrypointName, (_, _) => throw new Exception("Test exception"))
			.Build();
		await host.StartAsync();

		// Act
		host.WriteToStdinOnce("ping");

		// Assert
		Assert.Equal("pong", await host.ReadLineFromStdoutAsync());
	}

	[Fact]
	public async Task PlainInput()
	{
		// Arrange
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler(_ => new E5EResponse<string>("test"))
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("hello").SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync<string>();
		Assert.Equal("test", resp.Value);
	}

	[Fact]
	public async Task HeadersAreReceived()
	{
		// Arrange
		const string headerName = "Accept";
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler<string>(req =>
			{
				Assert.NotNull(req.RequestHeaders);

				req.RequestHeaders.TryGetValue(headerName, out var acceptHeader);
				return acceptHeader!;
			})
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("hello")
			.AddHeader(headerName, "application/json")
			.SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync<string>();
		Assert.Equal("application/json", resp);
	}

	[Fact]
	public async Task ParamsAreReceived()
	{
		// Arrange
		const string paramName = "myParam";
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler<List<string>>(req =>
			{
				Assert.NotNull(req.Params);

				req.Params.TryGetValue(paramName, out var parameter);
				return parameter!;
			})
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("test")
			.AddParam(paramName, "this is my parameter")
			.SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync<List<string>>();
		Assert.Equal("this is my parameter", resp.Value.FirstOrDefault());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		// Arrange
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler<string>(_ => "response")
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("request").SendTo(host);

		// Assert
		await host.StopAsync();
		Assert.Equal(@"+++{""data"":""response"",""type"":""text""}---", host.Stdout());
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormatting()
	{
		// Arrange
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler<string>(_ => "response")
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("request").SendTo(host);

		// Assert
		await host.StopAsync();
		Assert.Equal("---", host.Stderr());
	}
}
