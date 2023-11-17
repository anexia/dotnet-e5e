using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Functions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

/// <summary>
///     Provides several extensions to register e5e function entrypoints on a host.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class HostExtensions
{
	/// <summary>
	///     Register an entrypoint with the given handler type.
	/// </summary>
	/// <param name="host">The host.</param>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <typeparam name="T">The type of the handler.</typeparam>
	public static void RegisterEntrypoint<T>(this IHost host, string entrypoint) where T : IE5EFunctionHandler
	{
		RegisterEntrypoint(host, entrypoint, typeof(T));
	}

	/// <summary>
	///     Register an entrypoint with the given handler type.
	/// </summary>
	/// <param name="host">The host.</param>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="handlerType">The type of the class, needs to implement <see cref="IE5EFunctionHandler" />.</param>
	/// <exception cref="InvalidOperationException">
	///     Thrown if the handlerType is neither a class nor does it implement
	///     <see cref="IE5EFunctionHandler" />.
	/// </exception>
	public static void RegisterEntrypoint(this IHost host, string entrypoint, Type handlerType)
	{
		ArgumentNullException.ThrowIfNull(host);
		ArgumentNullException.ThrowIfNull(handlerType);

		if (!handlerType.IsClass || !handlerType.IsAssignableTo(typeof(IE5EFunctionHandler)))
			throw new InvalidOperationException($"The type {handlerType} is not suitable for registration.");

		var resolver = host.Services.GetRequiredService<E5EFunctionHandlerResolver>();
		resolver.Add(entrypoint, handlerType);
	}

	/// <summary>
	///     Register an entrypoint with the given inline handler.
	/// </summary>
	/// <param name="host">The host.</param>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="func">The handler.</param>
	public static void RegisterEntrypoint(this IHost host, string entrypoint, Func<E5ERequest, Task<E5EResponse>> func)
	{
		ArgumentNullException.ThrowIfNull(host);

		var resolver = host.Services.GetRequiredService<E5EFunctionHandlerResolver>();
		resolver.Add(entrypoint, new E5EInlineFunctionHandler(func));
	}
}
