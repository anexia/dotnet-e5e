using Anexia.E5E.Abstractions;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;

using Microsoft.Extensions.DependencyInjection;

namespace Anexia.E5E.Runtime;

/// <summary>
///     Resolves the implementation for the entrypoint that's passed during startup.
/// </summary>
internal sealed class E5EEntrypointBuilder : IE5EEntrypointBuilder
{
	private readonly Dictionary<string, Func<IServiceProvider, IE5EFunctionHandler>> _handlers = new();
	private readonly IServiceCollection _provider;

	public E5EEntrypointBuilder(IServiceCollection provider)
	{
		_provider = provider;
	}

	public void RegisterEntrypoint(string entrypoint, IE5EFunctionHandler handler)
	{
		if (_handlers.ContainsKey(entrypoint))
			throw new E5EEntrypointAlreadyRegisteredException(entrypoint);

		_handlers.Add(entrypoint, _ => handler);
	}

	public void RegisterEntrypoint(string entrypoint, Type handlerType)
	{
		if (!handlerType.IsClass || !handlerType.IsAssignableTo(typeof(IE5EFunctionHandler)))
			throw new InvalidOperationException($"The type {handlerType} is not suitable for registration.");

		if (_handlers.ContainsKey(entrypoint))
			throw new E5EEntrypointAlreadyRegisteredException(entrypoint);

		_provider.AddScoped(handlerType);
		_handlers.Add(entrypoint, svc => (svc.GetService(handlerType) as IE5EFunctionHandler)!);
	}

	public IE5EEntrypointResolver BuildResolver()
	{
		return new E5EEntrypointResolver(_handlers);
	}
}
