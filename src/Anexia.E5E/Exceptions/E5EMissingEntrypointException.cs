using System.Diagnostics.CodeAnalysis;

namespace Anexia.E5E.Exceptions;

/// <summary>
/// The exception that's thrown when the requested entrypoint cannot be found by the <see cref="DependencyInjection.E5EFunctionResolver"/>.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EMissingEntrypointException : E5EException
{
	/// <summary>
	/// The name of the entrypoint that's missing.
	/// </summary>
	public string Entrypoint { get; }

	internal E5EMissingEntrypointException(string entrypoint) : base($"There is no function registered for the entrypoint: {entrypoint}")
	{
		this.Entrypoint = entrypoint;
	}
}
