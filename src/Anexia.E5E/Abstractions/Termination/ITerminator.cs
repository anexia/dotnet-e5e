namespace Anexia.E5E.Abstractions.Termination;

/// <summary>
/// Because the usage of <see cref="Environment.Exit"/> would also crash our test processes, it is abstracted.
/// By default, the <see cref="EnvironmentTerminator"/> is used as implementation.
/// </summary>
internal interface ITerminator
{
	void Exit();
}
