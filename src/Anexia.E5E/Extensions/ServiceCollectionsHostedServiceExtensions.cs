using Anexia.E5E.Functions;

using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedMember.Global

namespace Anexia.E5E.Extensions;

/// <summary>
/// Extensions to register e5e functions on a <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionHostedServiceExtensions
{
	/// <summary>
	/// Searches for all implementations of <see cref="IE5EFunctionHandler"/> and registers them.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
	/// <returns>The original <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection AddE5EFunctionHandlers(this IServiceCollection services)
	{
		var handlerType = typeof(IE5EFunctionHandler);
		var allHandlers = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(s => s.GetTypes())
			.Where(p => handlerType.IsAssignableFrom(p));

		foreach (var h in allHandlers)
			services.AddSingleton(ServiceDescriptor.Scoped(handlerType, h));

		return services;
	}
}
