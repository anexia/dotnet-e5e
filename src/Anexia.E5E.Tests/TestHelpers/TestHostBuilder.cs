using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.TestHelpers;

public class TestHostBuilder
{
	private IHostBuilder _hb;
	private IHost? _host;

	private TestHostBuilder(ITestOutputHelper helper)
	{
		_hb = Host.CreateDefaultBuilder()
			.ConfigureLogging(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddConsole(options => options.FormatterName = "UnitTest")
					.AddConsoleFormatter<UnitTestConsoleFormatter, UnitTestConsoleFormatterOptions>(cfg =>
					{
						cfg.TestName = XunitContext.Context.MethodName;
					});
			});
	}


	public IHost Inner
		=> _host ?? throw new InvalidOperationException(
			"Cannot access host without building the application before using StartAsync.");

	public TestHostBuilder ConfigureEndpoints(Action<IE5EEntrypointBuilder> configure, string[]? args = default)
	{
		args ??= new[] { TestE5ERuntimeOptions.DefaultEntrypointName, "+++", "1", "---" };
		_hb = _hb.UseAnexiaE5E(args, configure);
		return this;
	}

	public Task StartAsync(int maximumLifetimeMs = 5000)
	{
		// In order to override the default implementation, we need to call this just before we build our host.
		_hb.ConfigureServices(services => services.AddSingleton<IConsoleAbstraction, TestConsoleAbstraction>());
		_host = _hb.Build();

		// Shutdown the host automatically after five seconds, there's no test that should run that long.
		var lifetime = Inner.Services.GetRequiredService<IHostApplicationLifetime>();
		lifetime.ApplicationStarted.Register(() =>
			{
				Task.Delay(maximumLifetimeMs).ContinueWith(_ => lifetime.StopApplication());
			}
		);
		return Inner.StartAsync();
	}

	public static TestHostBuilder New(ITestOutputHelper helper)
	{
		return new TestHostBuilder(helper);
	}

	public async Task<(string Stdout, string Stderr)> WriteOnceAsync(string message)
	{
		var console = Inner.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		console.WriteToStdin(message);
		await console.WaitForFirstWriteAsync();
		await Inner.StopAsync(TimeSpan.FromSeconds(3));

		var stdout = await GetStdoutAsync();
		var stderr = await GetStderrAsync();
		return (stdout, stderr);
	}

	public Task<(string Stdout, string Stderr)> WriteOnceAsync(
		Func<TestRequestBuilder, TestRequestBuilder> builderAction)
	{
		var builder = new TestRequestBuilder();
		builderAction.Invoke(builder);
		var evt = builder.BuildEvent();

		var req = new E5ERequest(evt, new E5EContext("test", DateTimeOffset.Now, true));
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);

		return WriteOnceAsync(json);
	}

	public async Task<E5EResponse> WriteRequestOnceAsync(Func<TestRequestBuilder, TestRequestBuilder> builder)
	{
		await WriteOnceAsync(builder);

		var resp = await ReadResponseAsync();
		return resp;
	}

	public Task<E5EResponse> WriteExampleRequestOnceAsync()
	{
		return WriteRequestOnceAsync(builder => builder.WithData("example"));
	}

	private async Task<E5EResponse> ReadResponseAsync()
	{
		var console = Inner.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		var options = Inner.Services.GetRequiredService<E5ERuntimeOptions>();
		var stdout = await console.GetStdoutAsync();
		var json = stdout
			.Replace(options.StdoutTerminationSequence, "")
			.Replace(options.DaemonExecutionTerminationSequence, "");

		var result = JsonSerializer.Deserialize<JsonElement>(json, E5EJsonSerializerOptions.Default);
		if (!result.TryGetProperty("result", out result))
			throw new InvalidOperationException("The response does not contain a 'result' property");

		return result.Deserialize<E5EResponse>(E5EJsonSerializerOptions.Default)!;
	}

	public Task StartWithTestEntrypointAsync(Func<E5ERequest, E5EResponse> handler)
	{
		ConfigureEndpoints(cfg =>
		{
			cfg.RegisterEntrypoint(TestE5ERuntimeOptions.DefaultEntrypointName, request =>
			{
				var resp = handler.Invoke(request);
				return Task.FromResult(resp);
			});
		});
		return StartAsync();
	}

	public Task<string> GetStderrAsync()
	{
		var console = Inner.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.GetStderrAsync();
	}

	public Task<string> GetStdoutAsync()
	{
		var console = Inner.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.GetStdoutAsync();
	}

	public Task WaitForShutdownAsync(CancellationToken token)
	{
		return Inner.WaitForShutdownAsync(token);
	}

	public Task StopAsync()
	{
		return _host?.StopAsync() ?? Task.CompletedTask;
	}
}
