namespace Anexia.E5E.Exceptions;

/// <summary>
/// Thrown if an entrypoint is registered twice or more.
/// </summary>
public class E5EEntrypointAlreadyRegisteredException : E5EException
{
	/// <summary>
	/// The entrypoint that is already existing.
	/// </summary>
	public string Entrypoint { get; }

	internal E5EEntrypointAlreadyRegisteredException(string entrypoint)
		: base($"The entrypoint {entrypoint} is already registered.")
	{
		this.Entrypoint = entrypoint;
	}
}
