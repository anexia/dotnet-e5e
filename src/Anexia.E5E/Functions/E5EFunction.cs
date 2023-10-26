namespace Anexia.E5E.Functions;

/// <summary>
/// Defines methods for objects that provide function handlers.
/// </summary>
public interface IE5EFunction
{
	/// <summary>
	/// The name of the entrypoint that's handled by this implementation.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Runs the function with the given request.
	/// </summary>
	/// <param name="request">The current request with all the provided metadata.</param>
	/// <param name="cancellationToken">Used for cancelling the operation on shutdown.</param>
	/// <returns>The response that's serialized and returned to E5E.</returns>
	Task<E5EResponse> RunAsync(E5ERequest request, CancellationToken cancellationToken = default);
}
