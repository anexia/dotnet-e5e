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
	public static async Task WriteToStdinOnceAsync(this IHost host, string input)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		console.WriteToStdin(input);
		await console.WaitForFirstWriteAsync();
		await host.StopAsync(TimeSpan.FromSeconds(3));
	}

	public static Task WriteToStdinOnceAsync(this IHost host, E5EEvent evt)
	{
		var req = new E5ERequest(evt, new E5ERequestContext("test", DateTimeOffset.Now, true));
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);
		return host.WriteToStdinOnceAsync(json);
	}

	public static Task WriteToStdinOnceAsync(this IHost host, E5ERequest req)
	{
		var json = JsonSerializer.Serialize(req, E5EJsonSerializerOptions.Default);
		return host.WriteToStdinOnceAsync(json);
	}

	public static async Task<E5EResponse> ReadResponseAsync(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		var options = host.Services.GetRequiredService<E5ERuntimeOptions>();
		var stdout = await console.GetStdoutAsync();
		var json = stdout
			.Replace(options.StdoutTerminationSequence, "")
			.Replace(options.DaemonExecutionTerminationSequence, "");
		var resp = JsonSerializer.Deserialize<E5EResponse>(json, E5EJsonSerializerOptions.Default);

		// ReSharper disable once NullableWarningSuppressionIsUsed
		return resp!;
	}

	public static Task<string> GetStdoutAsync(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.GetStdoutAsync();
	}

	public static Task<string> GetStderrAsync(this IHost host)
	{
		var console = host.Services.GetRequiredService<IConsoleAbstraction>() as TestConsoleAbstraction ??
					  throw new InvalidOperationException("There's no console registered");
		return console.GetStderrAsync();
	}

	public static void SetTestEntrypoint(this IHost host, Func<E5ERequest, E5EResponse> func)
	{
		host.RegisterEntrypoint(TestE5ERuntimeOptions.DefaultEntrypointName, request => Task.FromResult(func(request)));
	}
}
