using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.DependencyInjection;
using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

/// <summary>
/// A generic-purpose application that's for usage with Anexia e5e Function-as-a-Service offering.
/// </summary>
public static class E5EApplication
{
	/// <summary>
	/// Creates the default builder by calling e5e-specific configuration methods and
	/// building on top of <see cref="Host.CreateDefaultBuilder()"/>.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	/// <returns>An e5e-specific application builder.</returns>
	public static IE5EHostBuilder CreateBuilder(string[] args)
	{
		var options = E5ERuntimeOptions.Parse(args);
		return new HostBuilderInner(options);
	}

	/// <summary>
	/// Creates the default builder by calling e5e-specific configuration methods and
	/// building on top of <see cref="Host.CreateDefaultBuilder()"/>.
	///
	/// This method requires a dedicated <see cref="E5ERuntimeOptions"/> record and should be handled with care.
	/// </summary>
	/// <param name="options">The runtime arguments.</param>
	/// <returns>An e5e-specific application builder.</returns>
	public static IE5EHostBuilder CreateBuilder(E5ERuntimeOptions options)
		=> new HostBuilderInner(options);

	private class HostBuilderInner : IE5EHostBuilder
	{
		private readonly IHostBuilder _hb;
		private Action<IHostBuilder> _configure;
		private E5ERuntimeOptions _options;

		public HostBuilderInner(E5ERuntimeOptions opts)
		{
			this._options = opts;
			this._hb = Host.CreateDefaultBuilder();
			this.Properties = new Dictionary<object, object>();

			_configure = hb => hb.ConfigureServices(services =>
			{
				services.AddHostedService<E5ECommunicationService>();
				services.AddSingleton<E5EFunctionHandlerResolver>();
				services.AddSingleton<IConsoleAbstraction, ConsoleAbstraction>();
				services.AddScoped<IE5EFunctionHandler>(svc =>
				{
					var resolver = svc.GetRequiredService<E5EFunctionHandlerResolver>();
					return resolver.Resolve();
				});
			});
		}

		#region IHostBuilder inherited

		public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			_configure += hb => hb.ConfigureHostConfiguration(configureDelegate);
			return this;
		}

		public IHostBuilder ConfigureAppConfiguration(
			Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			_configure += hb => hb.ConfigureAppConfiguration(configureDelegate);
			return this;
		}

		public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
		{
			_configure += hb => hb.ConfigureServices(configureDelegate);
			return this;
		}

#nullable disable // hack: dirty workaround to satisfy the compiler.
		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			IServiceProviderFactory<TContainerBuilder> factory)
		{
			_configure += hb => hb.UseServiceProviderFactory(factory);
			return this;
		}

		public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
			Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
		{
			_configure += hb => hb.UseServiceProviderFactory(factory);
			return this;
		}
#nullable restore

		public IHostBuilder ConfigureContainer<TContainerBuilder>(
			Action<HostBuilderContext, TContainerBuilder> configureDelegate)
		{
			_configure += hb => hb.ConfigureContainer(configureDelegate);
			return this;
		}

		IHost IHostBuilder.Build()
		{
			return Build();
		}

		public IDictionary<object, object> Properties { get; }

		#endregion

		public IE5EHostBuilder OverrideRuntimeOptions(
			Func<E5ERuntimeOptions, E5ERuntimeOptions> configureDelegate)
		{
			_options = configureDelegate.Invoke(this._options);
			return this;
		}

		public IE5EHost Build()
		{
			_hb.ConfigureServices(svc => svc.AddSingleton(_options));
			_configure.Invoke(_hb);
			var host = _hb.Build();

			return new E5EHost(host);
		}
	}

	private class E5EHost : IE5EHost
	{
		private readonly IHost _host;
		private readonly E5ERuntimeOptions _options;

		public E5EHost(IHost host)
		{
			_host = host;
			_options = this.Services.GetRequiredService<E5ERuntimeOptions>();
		}

		public void Dispose() => _host.Dispose();

		public Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (!_options.WriteMetadataOnStartup)
				return _host.StartAsync(cancellationToken);

			var metadata = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
			Console.Out.Write(metadata);

			// If we wrote the metadata, circumvent the default host mechanism as used by Run/RunAsync extensions
			// and just stop the application.
			var lifetime = _host.Services.GetService<IHostApplicationLifetime>();
			lifetime?.StopApplication();

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken = default)
		{
			// If we wrote the metadata, circumvent the default host mechanism as used by Run/RunAsync extensions.
			// Otherwise we might write additional logs to the output which is not as expected.
			if (_options.WriteMetadataOnStartup)
				return Task.CompletedTask;

			return _host.StopAsync(cancellationToken);
		}

		public IServiceProvider Services => _host.Services;

		/// <summary>
		/// Register an entrypoint with the given handler type.
		/// </summary>
		/// <param name="entrypoint">The name of the entrypoint.</param>
		/// <typeparam name="T">The type of the handler.</typeparam>
		public void RegisterEntrypoint<T>(string entrypoint) where T : IE5EFunctionHandler
		{
			var resolver = this.Services.GetRequiredService<E5EFunctionHandlerResolver>();
			var impl = (IE5EFunctionHandler)Services.GetRequiredService(typeof(T));

			resolver.Add(entrypoint, impl);
		}

		/// <summary>
		/// Register an entrypoint with the given inline handler.
		/// </summary>
		/// <param name="entrypoint">The name of the entrypoint.</param>
		/// <param name="func">The handler.</param>
		public void RegisterEntrypoint(string entrypoint, Func<E5ERequest, Task<E5EResponse>> func)
		{
			var resolver = this.Services.GetRequiredService<E5EFunctionHandlerResolver>();
			resolver.Add(entrypoint, new E5EInlineFunctionHandler(func));
		}
	}
}