using System.Reflection;
using System.Text.Json.Serialization;

namespace Anexia.E5E.Runtime;

public class E5ERuntimeMetadata
{
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

	[JsonPropertyName("runtime")] public string Runtime => "DotNet";

	[JsonPropertyName("runtime_version")] public string RuntimeVersion => Environment.Version.ToString();

	[JsonPropertyName("features")] public IReadOnlyList<string> Features => new[] { "keepalive" };
}
