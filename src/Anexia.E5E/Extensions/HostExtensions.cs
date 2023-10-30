using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Functions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

/// <summary>
/// Provides several extensions to register e5e function entrypoints on a host.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class HostExtensions
{
	/// <summary>
	/// Register an entrypoint with the given handler type.
	/// </summary>
	/// <param name="host">The host.</param>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <typeparam name="T">The type of the handler.</typeparam>
	public static void RegisterEntrypoint<T>(this IHost host, string entrypoint) where T : IE5EFunctionHandler
	{
		var resolver = host.Services.GetRequiredService<E5EFunctionHandlerResolver>();
		var impl = (IE5EFunctionHandler)host.Services.GetRequiredService(typeof(T));

		resolver.Add(entrypoint, impl);
	}

	/// <summary>
	/// Register an entrypoint with the given inline handler.
	/// </summary>
	/// <param name="host">The host.</param>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="func">The handler.</param>
	public static void RegisterEntrypoint(this IHost host, string entrypoint, Func<E5ERequest, Task<E5EResponse>> func)
	{
		var resolver = host.Services.GetRequiredService<E5EFunctionHandlerResolver>();
		resolver.Add(entrypoint, new E5EInlineFunctionHandler(func));
	}
}
