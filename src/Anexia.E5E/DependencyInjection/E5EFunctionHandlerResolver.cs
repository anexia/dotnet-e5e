using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;

namespace Anexia.E5E.DependencyInjection;

/// <summary>
///     Resolves the implementation for the entrypoint that's passed during startup.
/// </summary>
internal sealed class E5EFunctionHandlerResolver
{
	private readonly Dictionary<string, Func<IServiceProvider, IE5EFunctionHandler>> _handlers = new();
	private readonly E5ERuntimeOptions _options;

	public E5EFunctionHandlerResolver(E5ERuntimeOptions options)
	{
		_options = options;
	}

	public void Add(string entrypoint, IE5EFunctionHandler handler)
	{
		if (_handlers.ContainsKey(entrypoint))
			throw new E5EEntrypointAlreadyRegisteredException(entrypoint);

		_handlers.Add(entrypoint, _ => handler);
	}

	public void Add(string entrypoint, Type handler)
	{
		if (_handlers.ContainsKey(entrypoint))
			throw new E5EEntrypointAlreadyRegisteredException(entrypoint);

		_handlers.Add(entrypoint, svc => (svc.GetService(handler) as IE5EFunctionHandler)!);
	}

	public IE5EFunctionHandler ResolveFrom(IServiceProvider services)
	{
		if (!_handlers.TryGetValue(_options.Entrypoint, out var resolve))
			throw new E5EMissingEntrypointException(_options.Entrypoint);

		return resolve.Invoke(services);
	}
}
