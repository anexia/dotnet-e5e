namespace Anexia.E5E.Abstractions.Termination;

internal sealed class EnvironmentTerminator : ITerminator
{
	public void Exit() => Environment.Exit(0);
}
