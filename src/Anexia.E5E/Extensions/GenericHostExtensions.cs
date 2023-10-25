using System.Text.Json;

using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

public static class GenericHostExtensions
{
	public static void RunE5E(this IHost host) => host.RunE5EAsync().GetAwaiter().GetResult();

	public static Task RunE5EAsync(this IHost host, CancellationToken cancellationToken = default)
	{
		var runtime = host.Services.GetRequiredService<E5ERuntimeOptions>();
		if (!runtime.WriteMetadataOnStartup)
			return host.RunAsync(cancellationToken);

		var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
		var metadata = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
		Console.Out.Write(metadata);
		lifetime.StopApplication();

		return Task.CompletedTask;
	}
}
