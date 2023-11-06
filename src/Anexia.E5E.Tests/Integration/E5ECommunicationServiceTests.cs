using System;
using System.Collections.Generic;
using System.Linq;
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
		Assert.Equal("pong---", Host.GetStdout());
		Assert.Equal("---", Host.GetStderr());
	}

	[Fact]
	public async Task PlainInput()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("test"));
		await E5ERequestBuilder.New("hello").SendAndShutdownAsync(Host);

		var resp = Host.ReadResponse();
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

		var resp = Host.ReadResponse();
		Assert.Equal("application/json", resp.Text());
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

		var resp = Host.ReadResponse();
		Assert.Equal("this is my parameter", resp.As<List<string>>().FirstOrDefault());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(Host);

		Assert.Equal(@"+++{""data"":""response"",""type"":""text""}---", Host.GetStdout());
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormatting()
	{
		Host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(Host);

		Assert.Equal("---", Host.GetStderr());
	}
}
