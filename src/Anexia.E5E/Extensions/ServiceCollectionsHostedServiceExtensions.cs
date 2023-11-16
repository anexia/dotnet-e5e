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
	/// Registers an implementation of <see cref="IE5EFunctionHandler"/> for the usage with <see cref="HostExtensions.RegisterEntrypoint{T}"/> and scoped lifetime.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <typeparam name="T">The type to register.</typeparam>
	public static IServiceCollection AddFunctionHandler<T>(this IServiceCollection services)
		where T : class, IE5EFunctionHandler =>
		AddFunctionHandler(services, typeof(T));

	/// <summary>
	/// Registers an implementation of the given type for the usage with <see cref="HostExtensions.RegisterEntrypoint(Microsoft.Extensions.Hosting.IHost,string,System.Type)"/> and scoped lifetime.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="serviceType">The type of the service.</param>
	/// <exception cref="InvalidOperationException">Thrown if serviceType is not a class or does not implement <see cref="IE5EFunctionHandler"/>.</exception>
	public static IServiceCollection AddFunctionHandler(this IServiceCollection services, Type serviceType)
	{
		if (!serviceType.IsClass || !serviceType.IsAssignableTo(typeof(IE5EFunctionHandler)))
			throw new InvalidOperationException("The type " + serviceType + " is not suitable for registration.");

		var descriptor = ServiceDescriptor.Scoped(typeof(IE5EFunctionHandler), serviceType);
		services.Add(descriptor);
		return services;
	}
}
