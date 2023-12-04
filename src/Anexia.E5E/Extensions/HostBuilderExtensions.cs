using Anexia.E5E.Abstractions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

/// <summary>
///     Provides several extensions to setup <see cref="IHostBuilder" /> for the usage with Anexia E5E.
/// </summary>
public static class HostBuilderExtensions
{
	/// <summary>
	///     Configures the <see cref="IHost" /> to be usable with Anexia E5E.
	///     Any functions can be registered with the <paramref name="configure"/> action.
	/// </summary>
	/// <remarks>
	///     The required runtime arguments are automatically read from <see cref="Environment.GetCommandLineArgs" />.
	/// </remarks>
	/// <param name="hb">The host builder instance</param>
	/// <param name="configure">
	///     The delegate for configuring the <see cref="IE5EEntrypointBuilder" /> that will be used to construct the
	///     <see cref="IE5EEntrypointResolver" />.
	/// </param>
	/// <returns>A E5E-specific <see cref="IHostBuilder" /> that's is wrapping <paramref name="hb" />.</returns>
	public static IHostBuilder UseAnexiaE5E(this IHostBuilder hb, Action<IE5EEntrypointBuilder> configure)
	{
		return UseAnexiaE5E(hb, null, configure);
	}

	/// <summary>
	///     Configures the <see cref="IHost" /> to be usable with Anexia E5E.
	///     Any functions can be registered with the <paramref name="configure"/> action.
	/// </summary>
	/// <remarks>
	///     This method should be handled with care, as it can cause unexpected compatibility issues. It's recommended to
	///     always use <see cref="UseAnexiaE5E(Microsoft.Extensions.Hosting.IHostBuilder,System.Action{Anexia.E5E.Abstractions.IE5EEntrypointBuilder})" />,
	///	    with tests being the exception.
	/// </remarks>
	/// <param name="hb">The host builder instance</param>
	/// <param name="args">The startup arguments that usually are given by the E5E engine.</param>
	/// <param name="configure">
	///     The delegate for configuring the <see cref="IE5EEntrypointBuilder" /> that will be used to construct the
	///     <see cref="IE5EEntrypointResolver" />.
	/// </param>
	/// <returns>A E5E-specific <see cref="IHostBuilder" /> that's is wrapping <paramref name="hb" />.</returns>
	public static IHostBuilder UseAnexiaE5E(this IHostBuilder hb, string[]? args,
		Action<IE5EEntrypointBuilder> configure)
	{
		ArgumentNullException.ThrowIfNull(hb);

		// Environment.GetCommandLineArgs() *includes* the program name while args (passed via Main) doesn't, therefore
		// we skip the first element.
		args ??= Environment.GetCommandLineArgs()[1..];
		var opts = E5ERuntimeOptions.Parse(args);

		hb.ConfigureServices((_, services) =>
		{
			var endpoints = new E5EEntrypointBuilder(services);
			configure.Invoke(endpoints);
			services.AddSingleton(endpoints.BuildResolver());
		});

		return new E5EHostBuilderWrapper(hb, opts);
	}

	private sealed class E5EHostBuilderWrapper : IHostBuilder
	{
		private readonly IHostBuilder _inner;

		public E5EHostBuilderWrapper(IHostBuilder inner, E5ERuntimeOptions runtimeOptions)
		{
			_inner = inner;
			_inner.ConfigureServices((_, services) =>
			{
				services.AddHostedService<E5ECommunicationService>();
				services.AddSingleton<IConsoleAbstraction, ConsoleAbstraction>();
				services.AddSingleton(runtimeOptions);
				services.AddScoped<IE5EFunctionHandler>(svc =>
					svc.GetRequiredService<IE5EEntrypointResolver>().Resolve(svc));
			});
		}


		public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			_inner.ConfigureHostConfiguration(configureDelegate);
			return this;
		}


		public IHostBuilder ConfigureAppConfiguration(
			Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			_inner.ConfigureAppConfiguration(configureDelegate);
			return this;
		}


		public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			_inner.ConfigureServices(configureDelegate);
			return this;
		}


		public IHost Build()
		{
			return new E5EHostWrapper(_inner.Build());
		}

		public IDictionary<object, object> Properties => _inner.Properties;
#nullable disable
		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			IServiceProviderFactory<TContainerBuilder> factory)
		{
			_inner.UseServiceProviderFactory(factory);
			return this;
		}

		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
		{
			_inner.UseServiceProviderFactory(factory);
			return this;
		}

		public IHostBuilder ConfigureContainer<TContainerBuilder>(
			Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			_inner.ConfigureContainer(configureDelegate);
			return this;
		}
	}
}
