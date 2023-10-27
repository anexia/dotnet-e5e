using Anexia.E5E.Runtime;

using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

/// <summary>
/// Provides methods for building a e5e host.
/// </summary>
public interface IE5EHostBuilder : IHostBuilder
{
	/// <summary>
	/// Configure the runtime options for this function call.
	/// This should be handled with care, as the defaults are set during startup by e5e.
	/// </summary>
	/// <param name="configureDelegate">The delegate to configure the options.</param>
	IE5EHostBuilder OverrideRuntimeOptions(Func<E5ERuntimeOptions, E5ERuntimeOptions> configureDelegate);

	/// <summary>
	/// Builds a new host.
	/// </summary>
	new IE5EHost Build();
}
