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
			.WithDefaultHandler(_ => E5EResponse.From("test"))
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("hello").SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync();
		Assert.Equal("test", resp.Text());
	}

	[Fact]
	public async Task HeadersAreReceived()
	{
		// Arrange
		const string headerName = "Accept";
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler(req =>
			{
				Assert.NotNull(req.Event?.RequestHeaders);

				req.Event.RequestHeaders.TryGetValue(headerName, out var acceptHeader);
				return E5EResponse.From(acceptHeader!);
			})
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("hello")
			.AddHeader(headerName, "application/json")
			.SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync();
		Assert.Equal("application/json", resp.Text());
	}

	[Fact]
	public async Task ParamsAreReceived()
	{
		// Arrange
		const string paramName = "myParam";
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler(req =>
			{
				Assert.NotNull(req.Event?.Params);

				req.Event.Params.TryGetValue(paramName, out var parameter);
				return E5EResponse.From(parameter!);
			})
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("test")
			.AddParam(paramName, "this is my parameter")
			.SendTo(host);

		// Assert
		var resp = await host.ReadResponseFromStdoutAsync();
		Assert.Equal("this is my parameter", resp.As<List<string>>().FirstOrDefault());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		// Arrange
		await using var host = E5EHostBuilder.New(_outputHelper)
			.WithDefaultHandler(_ => E5EResponse.From("response"))
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
			.WithDefaultHandler(_ => E5EResponse.From("response"))
			.Build();
		await host.StartAsync();

		// Act
		E5ERequestBuilder.New("request").SendTo(host);

		// Assert
		await host.StopAsync();
		Assert.Equal("---", host.Stderr());
	}
}
