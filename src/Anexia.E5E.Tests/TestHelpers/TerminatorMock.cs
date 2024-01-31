using Anexia.E5E.Abstractions.Termination;

namespace Anexia.E5E.Tests.TestHelpers;

public class TerminatorMock : ITerminator
{
	public void Exit() { Called = true; }

	public bool Called { get; private set; }
}
