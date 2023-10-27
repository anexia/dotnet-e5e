using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;

namespace Anexia.E5E.DependencyInjection;

/// <summary>
/// Resolves the implementation for the entrypoint that's passed during startup.
/// </summary>
internal class E5EFunctionHandlerResolver
{
	private readonly E5ERuntimeOptions _options;
	private readonly Dictionary<string, IE5EFunctionHandler> _handlers = new();

	public E5EFunctionHandlerResolver(E5ERuntimeOptions options)
	{
		_options = options;
	}

	public void Add(string entrypoint, IE5EFunctionHandler handler)
	{
		if (_handlers.ContainsKey(entrypoint))
			throw new E5EEntrypointAlreadyRegisteredException(entrypoint);

		_handlers.Add(entrypoint, handler);
	}

	public IE5EFunctionHandler Resolve()
	{
		if (!_handlers.TryGetValue(_options.Entrypoint, out var handler))
			throw new E5EMissingEntrypointException(_options.Entrypoint);

		return handler;
	}
}
