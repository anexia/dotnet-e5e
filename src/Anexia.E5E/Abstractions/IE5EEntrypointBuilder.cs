using Anexia.E5E.Functions;

namespace Anexia.E5E.Abstractions;

/// <summary>
///     A builder interface used by <see cref="Extensions.HostBuilderExtensions" /> to enable an easy setup of E5E
///     handlers.
/// </summary>
public interface IE5EEntrypointBuilder
{
	/// <summary>
	///     Register an entrypoint with the given handler type.
	///     The handler is automatically registered as scoped dependency in the backing <see cref="IServiceProvider" />.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="handlerType">The type of the class, needs to implement <see cref="IE5EFunctionHandler" />.</param>
	/// <exception cref="InvalidOperationException">
	///     Thrown if the handlerType is neither a class nor does it implement
	///     <see cref="IE5EFunctionHandler" />.
	/// </exception>
	void RegisterEntrypoint(string entrypoint, Type handlerType);

	/// <summary>
	///     Register an entrypoint with the given handler type.
	///     The handler is automatically registered as scoped dependency in the backing <see cref="IServiceProvider" />.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <typeparam name="T">The type of the handler.</typeparam>
	void RegisterEntrypoint<T>(string entrypoint) where T : IE5EFunctionHandler
	{
		RegisterEntrypoint(entrypoint, typeof(T));
	}


	/// <summary>
	///     Registers a specific implementation of the handler for the given entrypoint.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="handler">The type of the class, needs to implement <see cref="IE5EFunctionHandler" />.</param>
	void RegisterEntrypoint(string entrypoint, IE5EFunctionHandler handler);

	/// <summary>
	///     Register an entrypoint with the given inline handler.
	/// </summary>
	/// <param name="entrypoint">The name of the entrypoint.</param>
	/// <param name="func">The handler.</param>
	void RegisterEntrypoint(string entrypoint, Func<E5ERequest, Task<E5EResponse>> func)
	{
		RegisterEntrypoint(entrypoint, new E5EInlineFunctionHandler(func));
	}
}
