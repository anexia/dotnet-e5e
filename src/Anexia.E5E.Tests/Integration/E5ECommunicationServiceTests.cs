using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Builders;
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
		Host.SetTestEntrypoint(_ => throw new Exception("This method should not have been called"));
		await Host.WriteToStdinOnceAsync("ping");

		var stdout = await Host.GetStdoutAsync();
		Assert.Equal("pong---", stdout);

		var stderr = await Host.GetStderrAsync();
		Assert.Equal("---", stderr);
	}

	[Fact]
	public async Task PlainInput()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("test"));
		await E5ERequestBuilder.New("hello").SendAndShutdownAsync(Host);

		var resp = await Host.ReadResponseAsync();
		Assert.Equal("test", resp.Text());
	}

	[Fact]
	public async Task HeadersAreReceived()
	{
		const string headerName = "Accept";
		Host.SetTestEntrypoint(req =>
		{
			Assert.NotNull(req.Event.RequestHeaders);

			req.Event.RequestHeaders.TryGetValue(headerName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!);
		});

		await E5ERequestBuilder.New("hello")
			.AddHeader(headerName, "application/json")
			.SendAndShutdownAsync(Host);

		var resp = await Host.ReadResponseAsync();
		Assert.Equal("application/json", resp.Text());
	}

	[Fact]
	public async Task DataIsReceived()
	{
		Host.SetTestEntrypoint(req =>
		{
			Assert.NotNull(req.Context.Data);
			Assert.Equal("data", req.Context.Data.ToString());

			return E5EResponse.From("success");
		});


		var evt = E5ERequestBuilder.New("hello").Build();
		var ctx = new E5EContext("test", DateTimeOffset.Now, true,
			JsonSerializer.SerializeToElement("data"));

		await Host.WriteToStdinOnceAsync(new E5ERequest(evt, ctx));
	}

	[Fact]
	public async Task ParamsAreReceived()
	{
		const string parameterName = "myParam";
		Host.SetTestEntrypoint(req =>
		{
			Assert.NotNull(req.Event.Params);

			req.Event.Params.TryGetValue(parameterName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!);
		});


		await E5ERequestBuilder.New("hello")
			.AddParam(parameterName, "this is my parameter")
			.SendAndShutdownAsync(Host);

		var resp = await Host.ReadResponseAsync();
		Assert.Equal("this is my parameter", resp.As<List<string>>().FirstOrDefault());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(Host);

		var stdout = await Host.GetStdoutAsync();
		Assert.Equal(@"+++{""data"":""response"",""type"":""text""}---", stdout);
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormatting()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(Host);

		var stderr = await Host.GetStderrAsync();
		Assert.Equal("---", stderr);
	}
}
