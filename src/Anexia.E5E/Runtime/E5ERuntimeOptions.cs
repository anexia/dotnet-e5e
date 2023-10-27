using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Exceptions;

namespace Anexia.E5E.Runtime;

/// <summary>
/// Contains information about the startup parameters.
/// </summary>
/// <param name="Entrypoint">The name of the entrypoint that the <seealso cref="E5EFunctionHandlerResolver"/>  is searching for.</param>
/// <param name="StdoutTerminationSequence">Sequence that's written to differentiate responses from generic logs.</param>
/// <param name="DaemonExecutionTerminationSequence">Sequence that's written on shutdown.</param>
/// <param name="KeepAlive">Whether to keep the function alive after the first execution or not.</param>
/// <param name="WriteMetadataOnStartup">True if the <seealso cref="E5ERuntimeMetadata"/> should be written on startup.</param>
public record E5ERuntimeOptions(
	string Entrypoint,
	string StdoutTerminationSequence,
	string DaemonExecutionTerminationSequence,
	bool KeepAlive,
	bool WriteMetadataOnStartup = false)
{
	internal static readonly E5ERuntimeOptions WriteMetadata = new("", "", "", false, true);

	internal static E5ERuntimeOptions Parse(string[] args)
	{
		if (args.Length == 0)
			throw new E5EMissingArgumentsException("There were no arguments given.");

		if (args[0] == "metadata")
			return WriteMetadata;

		if (args.Length != 4)
			throw new E5EMissingArgumentsException($"Expected exactly four arguments given, got {args.Length}");

		var entrypoint = args[0];
		var stdoutTerminationSequence = args[1].Replace("\\0", "\0");
		var keepAlive = args[2] == "1";
		var daemonExecutionSequence = args[3].Replace("\\0", "\0");
		return new E5ERuntimeOptions(entrypoint, stdoutTerminationSequence, daemonExecutionSequence, keepAlive);
	}
}
