using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Anexia.E5E.Runtime;

/// <summary>
///     Runtime metadata that's used internally by E5E to get basic information about the environment.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public sealed record E5ERuntimeMetadata
{
	private E5ERuntimeMetadata()
	{
		// We try to fetch the informational version from the generated AssemblyInfo. Because SourceLink appends the
		// commit hash, we have to trim it afterwards.
		var version = typeof(E5ERuntimeMetadata).Assembly
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			?.InformationalVersion ?? "0.0.0+unknown";

		LibraryVersion = version[..version.IndexOf('+')];
	}

	/// <summary>
	///     The current instance of the metadata.
	/// </summary>
	public static readonly E5ERuntimeMetadata Current = new();

	/// <summary>
	///     The installed version of this NuGet library.
	/// </summary>
	public string LibraryVersion { get; }

	/// <summary>
	///     The runtime this function is running in.
	/// </summary>
	public string Runtime => "DotNet";

	/// <summary>
	///     The runtime version of this function.
	/// </summary>
	public string RuntimeVersion => Environment.Version.ToString();

	/// <summary>
	///     A list of supported features.
	/// </summary>
	public IReadOnlyList<string> Features => new[] { "keepalive" };
}
