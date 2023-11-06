using System;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Tests.TestHelpers;

public static class HostExtensions
{
	public static Task WriteToStdinOnceAsync(this IHost host, string input)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		console.WriteToStdin(input);
		console.WaitForFirstWriteAction();
		return host.StopAsync(TimeSpan.FromSeconds(3));
	}

	public static Task WriteToStdinOnceAsync(this IHost host, E5EEvent evt)
	{
		var req = new E5ERequest(evt, new E5ERequestContext("test", DateTimeOffset.Now, true));
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);
		return host.WriteToStdinOnceAsync(json);
	}

	public static E5EResponse ReadResponse(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		var options = host.Services.GetRequiredService<E5ERuntimeOptions>();

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

	public static string GetStdout(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.Stdout();
	}

	public static string GetStderr(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.Stderr();
	}

	public static void SetTestEntrypoint(this IHost host, Func<E5ERequest, E5EResponse> func)
	{
		host.RegisterEntrypoint(TestE5ERuntimeOptions.DefaultEntrypointName, request => Task.FromResult(func(request)));
	}
}
