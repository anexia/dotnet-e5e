using System.Diagnostics.CodeAnalysis;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

/// <summary>
/// The exception that is thrown when the execution of the user-provided handler fails.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class E5EFunctionExecutionFailedException : E5EException
{
	/// <summary>
	/// The execution context.
	/// </summary>
	public E5EContext Context { get; }

	/// <summary>
	/// The request that was provided to the function.
	/// </summary>
	public E5ERequest Request { get; }

	internal E5EFunctionExecutionFailedException(E5EContext context, E5ERequest request, Exception inner) : base("The execution of the function failed.", inner)
	{
		Context = context;
		Request = request;
	}
}
