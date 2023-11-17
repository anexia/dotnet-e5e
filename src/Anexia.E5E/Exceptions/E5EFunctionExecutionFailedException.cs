using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

/// <summary>
///     The exception that is thrown when the execution of the user-provided handler fails.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EFunctionExecutionFailedException : E5EException
{
	internal E5EFunctionExecutionFailedException(E5ERequest request, Exception inner)
		: base("The entrypoint handler for the function failed.", inner)
	{
		Request = request;
	}

	/// <summary>
	///     The executed request.
	/// </summary>
	public E5ERequest Request { get; }
}
