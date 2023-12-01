using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

internal sealed class E5EHostWrapper : IHost
{
	private readonly IConsoleAbstraction _console;
	private readonly IHost _host;
	private readonly E5ERuntimeOptions _options;

	public E5EHostWrapper(IHost host)
	{
		_host = host;
		_options = Services.GetRequiredService<E5ERuntimeOptions>();
		_console = Services.GetRequiredService<IConsoleAbstraction>();
	}

	public void Dispose()
	{
		_host.Dispose();
	}

	public async Task StartAsync(CancellationToken cancellationToken = default)
	{
		if (!_options.WriteMetadataOnStartup)
		{
			await _host.StartAsync(cancellationToken).ConfigureAwait(false);
			return;
		}

		string metadata = "";
#if NET8_0_OR_GREATER
		metadata = JsonSerializer.Serialize(new E5ERuntimeMetadata(),
			E5ESerializationContext.Default.E5ERuntimeMetadata);
#else
		metadata = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
#endif

		_console.Open();
		await _console.WriteToStdoutAsync(metadata).ConfigureAwait(false);
		_console.Close();

		// If we wrote the metadata, circumvent the default host mechanism as used by Run/RunAsync extensions
		// and just stop the application.
		_host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
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
