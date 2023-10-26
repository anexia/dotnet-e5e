namespace Anexia.E5E.Runtime;

/// <summary>
/// Contains information about the startup parameters.
/// </summary>
/// <param name="Entrypoint">The name of the entrypoint that the <seealso cref="DependencyInjection.E5EFunctionResolver"/>  is searching for.</param>
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
}
