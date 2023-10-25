using Anexia.E5E.Runtime;

namespace Anexia.E5E.Tests.Helpers;

public record TestE5ERuntimeOptions() : E5ERuntimeOptions(DefaultEntrypointName, "+++", "---", true)
{
	public const string DefaultEntrypointName = "MyFunction";
}
