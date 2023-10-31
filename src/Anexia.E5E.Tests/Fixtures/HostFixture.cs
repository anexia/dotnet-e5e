using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Builders;
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
	public IHost Host { get; }

	public HostFixture(ITestOutputHelper testOutput)
	{
		Host = E5EApplication.CreateBuilder(new TestE5ERuntimeOptions())
			.ConfigureHostOptions((_, hostOptions) => hostOptions.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
			.ConfigureServices(services => services.AddSingleton<IConsoleAbstraction, TestConsoleAbstraction>())
			.ConfigureLogging(lb => lb.AddXUnit(testOutput).AddDebug().SetMinimumLevel(LogLevel.Debug))
			.Build();
	}

	public Task InitializeAsync() => Host.StartAsync();
	public Task DisposeAsync() => Host.StopAsync();

	public Task WriteToStdinOnceAsync(string input)
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		console.WriteToStdin(input);
		return DisposeAsync();
	}

	public Task WriteToStdinOnceAsync(E5EEvent evt)
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");

		var req = new E5ERequest(evt, new E5ERequestContext("test", DateTimeOffset.Now, true));
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);
		console.WriteToStdin(json);
		return DisposeAsync();
	}

	public E5EResponse ReadResponse()
	{
		var console = Host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		var options = Host.Services.GetRequiredService<E5ERuntimeOptions>();

		var line = console.ReadLineFromStdout();
		while (!line.StartsWith(options.StdoutTerminationSequence))
		{
			line = console.ReadLineFromStdout();
		}

		// Remove the termination sequence from the line
		line = line[options.StdoutTerminationSequence.Length..];

		var resp = JsonSerializer.Deserialize<E5EResponse>(line, E5EJsonSerializerOptions.Default);

		// ReSharper disable once NullableWarningSuppressionIsUsed
		return resp!;
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
