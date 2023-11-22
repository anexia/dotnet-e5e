using Anexia.E5E.Abstractions;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;

using Microsoft.Extensions.DependencyInjection;

namespace Anexia.E5E.Runtime;

internal sealed class E5EEntrypointResolver : IE5EEntrypointResolver
{
	private readonly IReadOnlyDictionary<string, Func<IServiceProvider, IE5EFunctionHandler>> _handlers;

	public E5EEntrypointResolver(IReadOnlyDictionary<string, Func<IServiceProvider, IE5EFunctionHandler>> handlers)
	{
		_handlers = handlers;
	}

	public IE5EFunctionHandler Resolve(IServiceProvider services)
	{
		var options = services.GetRequiredService<E5ERuntimeOptions>();

		if (!_handlers.TryGetValue(options.Entrypoint, out var resolve))
			throw new E5EMissingEntrypointException(options.Entrypoint);

		return resolve.Invoke(services);
	}
}
