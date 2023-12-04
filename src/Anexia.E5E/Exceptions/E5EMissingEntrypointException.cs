using System.Diagnostics.CodeAnalysis;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     The exception that's thrown when the requested entrypoint cannot be found by the
///     <see cref="Abstractions.IE5EEntrypointResolver" />.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EMissingEntrypointException : E5EException
{
	internal E5EMissingEntrypointException(string entrypoint) : base(
		$"There is no function registered for the entrypoint: {entrypoint}")
	{
		Entrypoint = entrypoint;
	}

	/// <summary>
	///     The name of the entrypoint that's missing.
	/// </summary>
	public string Entrypoint { get; }
}
