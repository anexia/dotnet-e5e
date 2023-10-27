using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class HostFixture : IAsyncLifetime
{
	// ReSharper disable once MemberCanBePrivate.Global
	public IE5EHost Host { get; }

	public HostFixture(ITestOutputHelper testOutput)
	{
		Host = (IE5EHost)E5EApplication.CreateBuilder(new TestE5ERuntimeOptions())
			.ConfigureHostOptions((_, hostOptions) =>
				hostOptions.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
			.ConfigureServices(services => services.AddSingleton<IConsoleAbstraction, TestConsoleAbstraction>())
			.ConfigureLogging(lb => lb.AddXUnit(testOutput).AddDebug().SetMinimumLevel(LogLevel.Debug))
			.Build();
	}

	public Task InitializeAsync() => Host.StartAsync();
	public Task DisposeAsync() => Host.StopAsync();

	public void WriteToStdinAndClose(string input)
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		console.WriteToStdin(input);
		console.Close();
	}

	public void WriteToStdinAndClose(E5EEvent evt)
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");

		var req = new E5ERequest(evt, new E5ERequestContext("test", DateTimeOffset.Now, true));
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);
		console.WriteToStdin(json);
		console.Close();
	}

	public async Task<E5EResponse> ReadResponseFromStdoutAsync(TimeSpan? timeout = null)
	{
		timeout ??= TimeSpan.FromSeconds(3);
		using var cts = new CancellationTokenSource(timeout.Value);

		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		var options = Host.Services.GetRequiredService<E5ERuntimeOptions>();

		string? line = await console.ReadLineFromStdoutAsync(cts.Token);
		while (line != options.StdoutTerminationSequence)
		{
			line = await console.ReadLineFromStdoutAsync(cts.Token);
		}

		// The line is now the termination sequence, let's read the next line.
		line = await console.ReadLineFromStdoutAsync(cts.Token) ??
			   throw new InvalidOperationException("No next line found after termination sequence");

		var resp = JsonSerializer.Deserialize<E5EResponse>(line, E5EJsonSerializerOptions.Default);

		// ReSharper disable once NullableWarningSuppressionIsUsed
		return resp!;
	}

	public async Task<string> ReadLineFromStdoutAsync(TimeSpan? timeout = null)
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");

		timeout ??= TimeSpan.FromSeconds(3);
		using var cts = new CancellationTokenSource(timeout.Value);

		var line = await console.ReadLineFromStdoutAsync(cts.Token);
		return line ?? throw new TimeoutException("No valid line was read within the timeout.");
	}

	public string GetStdout()
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.Stdout();
	}

	public string GetStderr()
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.Stderr();
	}

	public void SetTestEntrypoint(Func<E5ERequest, E5EResponse> func)
	{
		Host.RegisterEntrypoint(TestE5ERuntimeOptions.DefaultEntrypointName, request => Task.FromResult(func(request)));
	}
}