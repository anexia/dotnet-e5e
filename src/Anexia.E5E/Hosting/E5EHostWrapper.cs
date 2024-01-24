using System.Text.Json;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Abstractions.Termination;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Hosting;

internal sealed class E5EHostWrapper : IHost
{
	private readonly IConsoleAbstraction _console;
	private readonly ITerminator _terminator;
	private readonly IHost _host;
	private readonly E5ERuntimeOptions _options;

	public E5EHostWrapper(IHost host)
	{
		_host = host;
		_options = Services.GetRequiredService<E5ERuntimeOptions>();
		_console = Services.GetRequiredService<IConsoleAbstraction>();
		_terminator = Services.GetService<ITerminator>() ?? new EnvironmentTerminator();
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

#if NET8_0_OR_GREATER
		var metadata = JsonSerializer.Serialize(E5ERuntimeMetadata.Current,
			E5ESerializationContext.Default.E5ERuntimeMetadata);
#else
		var metadata = JsonSerializer.Serialize(E5ERuntimeMetadata.Current, E5EJsonSerializerOptions.Default);
#endif

		_console.Open();
		await _console.WriteToStdoutAsync(metadata).ConfigureAwait(false);
		_console.Close();

		// After we wrote the metadata, close the application immediately(!). Any startup tasks that occur after the startup
		// (e.g. a long initialization task) won't be executed and the metadata is returned to e5e.
		_terminator.Exit();
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
