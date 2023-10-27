using Anexia.E5E.Hosting;

namespace Anexia.E5E.Exceptions;

/// <summary>
/// Thrown if a registration of a typed handler in <see cref="IE5EHost.RegisterEntrypoint{T}" /> cannot be found.
/// </summary>
public class E5EHandlerNotFoundException : E5EException
{
	/// <summary>
	/// The type of the handler.
	/// </summary>
	public Type HandlerType { get; }

	internal E5EHandlerNotFoundException(Type handlerType)
		: base($"The type {handlerType} has not been registered in ConfigureServices.")
	{
		HandlerType = handlerType;
	}
}
