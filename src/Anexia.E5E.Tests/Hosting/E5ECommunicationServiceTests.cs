using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Builders;
using Anexia.E5E.Tests.Fixtures;
using Anexia.E5E.Tests.Helpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Hosting;

public class E5ECommunicationServiceTests : IAsyncLifetime
{
	private readonly HostFixture _host;
	public E5ECommunicationServiceTests(ITestOutputHelper outputHelper) => _host = new HostFixture(outputHelper);

	public Task InitializeAsync() => _host.InitializeAsync();
	public Task DisposeAsync() => _host.DisposeAsync();

	[Fact]
	public async Task PingResponds()
	{
		_host.SetTestEntrypoint(_ => throw new Exception("This method should not have been called"));
		await _host.WriteToStdinOnceAsync("ping");
		Assert.Equal("pong---", _host.GetStdout());
		Assert.Equal("---", _host.GetStderr());
	}

	[Fact]
	public async Task PlainInput()
	{
		_host.SetTestEntrypoint(_ => E5EResponse.From("test"));
		await E5ERequestBuilder.New("hello").SendAndShutdownAsync(_host);

		var resp = _host.ReadResponse();
		Assert.Equal("test", resp.Text());
	}

	[Fact]
	public async Task HeadersAreReceived()
	{
		const string headerName = "Accept";
		_host.SetTestEntrypoint(req =>
		{
			Assert.NotNull(req.Event?.RequestHeaders);

			req.Event.RequestHeaders.TryGetValue(headerName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!);
		});

		await E5ERequestBuilder.New("hello")
			.AddHeader(headerName, "application/json")
			.SendAndShutdownAsync(_host);

		var resp = _host.ReadResponse();
		Assert.Equal("application/json", resp.Text());
	}

	[Fact]
	public async Task ParamsAreReceived()
	{
		const string parameterName = "myParam";
		_host.SetTestEntrypoint(req =>
		{
			Assert.NotNull(req.Event?.Params);

			req.Event.Params.TryGetValue(parameterName, out var acceptHeader);
			return E5EResponse.From(acceptHeader!);
		});


		await E5ERequestBuilder.New("hello")
			.AddParam(parameterName, "this is my parameter")
			.SendAndShutdownAsync(_host);

		var resp = _host.ReadResponse();
		Assert.Equal("this is my parameter", resp.As<List<string>>().FirstOrDefault());
	}

	[Fact]
	public async Task ShouldHaveCorrectStdoutFormatting()
	{
		_host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(_host);

		Assert.Equal(@"+++{""data"":""response"",""type"":""text""}---", _host.GetStdout());
	}

	[Fact]
	public async Task ShouldHaveCorrectStderrFormatting()
	{
		_host.SetTestEntrypoint(_ => E5EResponse.From("response"));
		await E5ERequestBuilder.New("request").SendAndShutdownAsync(_host);

		Assert.Equal("---", _host.GetStderr());
	}
}
