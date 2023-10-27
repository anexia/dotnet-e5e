using Anexia.E5E.Functions;

using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

/// <summary>
/// Provides method for hosting e5e functions. 
/// </summary>
public interface IE5EHost : IHost
{
	/// <summary>
	/// Register an entrypoint with the given handler type.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <typeparam name="T">The type of the handler.</typeparam>
	void RegisterEntrypoint<T>(string entrypoint) where T : IE5EFunctionHandler;

	/// <summary>
	/// Register an entrypoint with the given inline handler.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="func">The handler.</param>
	void RegisterEntrypoint(string entrypoint, Func<E5ERequest, Task<E5EResponse>> func);
}
