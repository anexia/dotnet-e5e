namespace Anexia.E5E.Runtime;

public record E5ERuntimeOptions(string Entrypoint, string StdoutTerminationSequence,
	string DaemonExecutionTerminationSequence, bool KeepAlive);
