using System.Diagnostics.CodeAnalysis;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     Thrown if an entrypoint is registered twice or more.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EEntrypointAlreadyRegisteredException : E5EException
{
	internal E5EEntrypointAlreadyRegisteredException(string entrypoint)
		: base($"The entrypoint {entrypoint} is already registered.")
	{
		Entrypoint = entrypoint;
	}

	/// <summary>
	///     The entrypoint that is already existing.
	/// </summary>
	public string Entrypoint { get; }
}
