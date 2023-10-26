using System.Text.Json;

using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Extensions;

/// <summary>
/// e5e-specific extensions to run a <see cref="IHost"/>. This MUST be used instead of <seealso cref="HostingAbstractionsHostExtensions.RunAsync"/>
/// in order to support the ability to return the <see cref="E5ERuntimeMetadata"/> on startup.
/// </summary>
public static class GenericHostExtensions
{
	/// <summary>
	/// Runs an application and block the calling thread until host shutdown.
	/// </summary>
	/// <param name="host"></param>
	public static void RunE5E(this IHost host) => host.RunE5EAsync().GetAwaiter().GetResult();

	/// <summary>
	/// Runs an application and returns a <see cref="Task"/> that only completes when the token is triggered or shutdown
	/// is triggered. The host instance is disposed of after running.
	///
	/// If the e5e engine requests the metadata, it's written to <seealso cref="Console.Out"/> and the application exists
	/// immediately.
	/// </summary>
	/// <param name="host">The <see cref="IHost"/> to run.</param>
	/// <param name="cancellationToken">The token to trigger shutdown.</param>
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
