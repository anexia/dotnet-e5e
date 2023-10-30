using System.Diagnostics.CodeAnalysis;

namespace Anexia.E5E.Functions;

/// <summary>
/// Defines methods for objects that provide function handlers.
/// </summary>
public interface IE5EFunctionHandler
{
	/// <summary>
	/// Runs the function with the given request.
	/// </summary>
	/// <param name="request">The current request with all the provided metadata.</param>
	/// <param name="cancellationToken">Used for cancelling the operation on shutdown.</param>
	/// <returns>The response that's serialized and returned to E5E.</returns>
	[SuppressMessage("ReSharper", "UnusedParameter.Global")]
	Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default);
}
