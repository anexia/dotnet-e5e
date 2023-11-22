using Anexia.E5E.Functions;

namespace Anexia.E5E.Abstractions;

/// <summary>
/// Resolves the implementation type for a given entrypoint.
/// </summary>
public interface IE5EEntrypointResolver
{
	/// <summary>
	/// Resolves the implementation, optionally using the given <see cref="IServiceProvider"/>.
	/// The entrypoint is determined by querying <see cref="Runtime.E5ERuntimeOptions"/> from the <paramref name="services"/>.
	/// </summary>
	/// <param name="services">The <see cref="IServiceProvider"/> that was built.</param>
	/// <returns>The implementation.</returns>
	IE5EFunctionHandler Resolve(IServiceProvider services);
}
