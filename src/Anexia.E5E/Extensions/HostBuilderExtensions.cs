using Anexia.E5E.Abstractions;
using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

/// <summary>
/// Provides several extensions to setup <see cref="IHostBuilder"/> for the usage with Anexia e5e.
/// </summary>
public static class HostBuilderExtensions
{
	/// <summary>
	/// Adds the e5e support to the given <see cref="IHostBuilder"/>.
	/// </summary>
	/// <param name="hb">The host builder.</param>
	/// <param name="args">The command line arguments.</param>
	/// <exception cref="E5EMissingArgumentsException">Thrown if there are missing arguments.</exception>
	public static IHostBuilder UseAnexiaE5E(this IHostBuilder hb, string[] args)
		=> UseAnexiaE5E(hb, E5ERuntimeOptions.Parse(args));

	/// <summary>
	/// Adds the e5e support to the given <see cref="IHostBuilder"/> with the given <see cref="E5ERuntimeOptions"/>.
	/// This extension method should be used with care and it's almost always better to use <see cref="UseAnexiaE5E(Microsoft.Extensions.Hosting.IHostBuilder,string[])"/> instead.
	/// It is provided by us to test your handlers in integration tests easily.
	/// </summary>
	/// <param name="hb">The host builder.</param>
	/// <param name="options">The runtime options.</param>
	/// <exception cref="E5EMissingArgumentsException">Thrown if there are missing arguments.</exception>
	public static IHostBuilder UseAnexiaE5E(this IHostBuilder hb, E5ERuntimeOptions options)
		=> new E5EHostBuilderWrapper(hb, options);

	private class E5EHostBuilderWrapper : IHostBuilder
	{
		private readonly IHostBuilder _inner;

		public E5EHostBuilderWrapper(IHostBuilder inner, E5ERuntimeOptions runtimeOptions)
		{
			_inner = inner;
			ConfigureServices((_, services) =>
			{
				services.AddHostedService<E5ECommunicationService>();
				services.AddSingleton<E5EFunctionHandlerResolver>();
				services.AddSingleton<IConsoleAbstraction, ConsoleAbstraction>();
				services.AddSingleton(runtimeOptions);
				services.AddScoped<IE5EFunctionHandler>(svc =>
				{
					var resolver = svc.GetRequiredService<E5EFunctionHandlerResolver>();
					return resolver.Resolve();
				});
			});
		}


		public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
			_inner.ConfigureHostConfiguration(configureDelegate);


		public IHostBuilder ConfigureAppConfiguration(
			Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
			_inner.ConfigureAppConfiguration(configureDelegate);


		public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
			_inner.ConfigureServices(configureDelegate);
#nullable disable
		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			IServiceProviderFactory<TContainerBuilder> factory) =>
			_inner.UseServiceProviderFactory(factory);

		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
			_inner.UseServiceProviderFactory(factory);

		public IHostBuilder ConfigureContainer<TContainerBuilder>(
			Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
			_inner.ConfigureContainer(configureDelegate);
#nullable restore


		public IHost Build() => new E5EHostWrapper(_inner.Build());

		public IDictionary<object, object> Properties => _inner.Properties;
	}
}
