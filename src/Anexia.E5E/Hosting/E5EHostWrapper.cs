using System.Text.Json;

using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

internal sealed class E5EHostWrapper : IHost
{
	private readonly IHost _host;
	private readonly E5ERuntimeOptions _options;

	public E5EHostWrapper(IHost host)
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

		// For the remaining logs, just send them into the void.
		// This is not an elegant solution, but since the host shouldn't have been started in the first place,
		// it's a workaround.
		Console.SetOut(new StreamWriter(Stream.Null));

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
		return _options.WriteMetadataOnStartup
			? Task.CompletedTask
			: _host.StopAsync(cancellationToken);
	}

	public IServiceProvider Services => _host.Services;
}
