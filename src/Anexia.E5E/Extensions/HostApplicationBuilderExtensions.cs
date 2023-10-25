using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anexia.E5E.Extensions;

public static class HostApplicationBuilderExtensions
{
	public static IHostBuilder ConfigureE5E(this IHostBuilder hb, string[] args)
	{
		// If you're working on the argument indices, please be aware of the following fact:
		// 
		// > Unlike C and C++, the name of the program is not treated as the first command-line argument in the args array [...]
		// > https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/main-command-line

		if (args.Length == 0)
			throw new E5EMissingArgumentsException(
				$"There were no arguments given to the {nameof(ConfigureE5E)} method.");

		hb.ConfigureServices((_, services) =>
		{
			services.TryAddEntrypointServiceResolver();
			services.TryAddSingleton<IConsoleAbstraction, ConsoleAbstraction>();
		});

		if (args[0] == "metadata")
			return ConfigureE5E(hb, E5ERuntimeOptions.WriteMetadata);

		if (args.Length != 4)
			throw new E5EMissingArgumentsException($"Expected exactly four arguments given, got {args.Length}");


		var entrypoint = args[0];
		var stdoutTerminationSequence = args[1].Replace("\\0", "\0");
		var keepAlive = args[2] == "1";
		var daemonExecutionSequence = args[3].Replace("\\0", "\0");
		var options = new E5ERuntimeOptions(entrypoint, stdoutTerminationSequence, daemonExecutionSequence, keepAlive);

		return ConfigureE5E(hb, options);
	}

	public static IHostBuilder ConfigureE5E(this IHostBuilder hb, E5ERuntimeOptions options)
	{
		hb.ConfigureServices((_, services) =>
		{
			services.TryAddEntrypointServiceResolver();
			services.TryAddSingleton<IConsoleAbstraction, ConsoleAbstraction>();
			services.TryAddSingleton(options);
			services.AddHostedService<E5ECommunicationService>();
		});

		return hb;
	}
}
