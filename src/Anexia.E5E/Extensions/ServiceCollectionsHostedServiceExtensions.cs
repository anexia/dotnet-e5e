using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Anexia.E5E.Extensions;

/// <summary>
/// Extensions to register e5e functions on a <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionHostedServiceExtensions
{
	/// <summary>
	/// Add an <see cref="IE5EFunction"/> registration for the given type. The name is automatically derived from the name of <typeparamref name="TFunction"/>.
	/// </summary>
	/// <typeparam name="TFunction">An <see cref="IE5EFunction"/> to register.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
	/// <returns>The original <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection AddE5EFunction<TFunction>(this IServiceCollection services)
		where TFunction : class, IE5EFunction
	{
		services.TryAddEntrypointServiceResolver();
		services.AddScoped<IE5EFunction, TFunction>();
		return services;
	}

	/// <summary>
	/// Add an inline <see cref="IE5EFunction"/> registration for the given type and name.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
	/// <param name="name">The name of the function.</param>
	/// <param name="func">The implementation of the function.</param>
	/// <returns>The original <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection AddE5EFunction(this IServiceCollection services, string name,
		Func<E5ERequest, CancellationToken, Task<E5EResponse>> func)
	{
		services.TryAddEntrypointServiceResolver();
		services.Add(ServiceDescriptor.Scoped<IE5EFunction>(_ => new E5EFuncFunction(name, func)));
		return services;
	}

	internal static void TryAddEntrypointServiceResolver(this IServiceCollection services)
	{
		services.TryAddScoped<E5EFunctionResolver>(svc => () =>
		{
			var options = svc.GetRequiredService<E5ERuntimeOptions>();
			var func = svc.GetServices<IE5EFunction>()
				.SingleOrDefault(x => x.Name.Equals(options.Entrypoint, StringComparison.InvariantCulture));

			return func ?? throw new E5EMissingEntrypointException(options.Entrypoint);
		});
	}
}
