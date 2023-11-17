using Anexia.E5E.Functions;

namespace WithDependencyInjection;

public class Handler : IE5EFunctionHandler
{
	private readonly ILogger<Handler> _logger;

	public Handler(ILogger<Handler> logger)
	{
		_logger = logger;
	}

	/// <summary>
	///     Runs the function with the given request.
	/// </summary>
	/// <param name="request">The current request with all the provided metadata.</param>
	/// <param name="cancellationToken">Used for cancelling the operation on shutdown.</param>
	/// <returns>The response that's serialized and returned to E5E.</returns>
	public async Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Running for {Request}", request);
		return E5EResponse.From(DateTimeOffset.Now.ToString());
	}
}
