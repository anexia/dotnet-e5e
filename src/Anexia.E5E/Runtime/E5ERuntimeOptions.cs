namespace Anexia.E5E.Runtime;

public record E5ERuntimeOptions(string Entrypoint, string StdoutTerminationSequence,
	string DaemonExecutionTerminationSequence, bool KeepAlive, bool WriteMetadataOnStartup = false)
{
	internal static readonly E5ERuntimeOptions WriteMetadata = new("", "", "", false, true);
}
