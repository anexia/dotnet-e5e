using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Runtime;

/// <summary>
/// Runtime metadata that's used internally by e5e to get basic information about the environment.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public record E5ERuntimeMetadata
{
	/// <summary>
	/// The installed version of this NuGet library.
	/// </summary>
	[JsonPropertyName("library_version")]
	public string LibraryVersion
	{
		get
		{
			var version = typeof(E5ERuntimeMetadata).Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion ?? "0.0.1";
			return version;
		}
	}

	/// <summary>
	/// The runtime this function is running in.
	/// </summary>
	[JsonPropertyName("runtime")] public string Runtime => "DotNet";

	/// <summary>
	/// The runtime version of this function.
	/// </summary>
	[JsonPropertyName("runtime_version")] public string RuntimeVersion => Environment.Version.ToString();

	/// <summary>
	/// A list of supported features.
	/// </summary>
	[JsonPropertyName("features")] public IReadOnlyList<string> Features => new[] { "keepalive" };
}
