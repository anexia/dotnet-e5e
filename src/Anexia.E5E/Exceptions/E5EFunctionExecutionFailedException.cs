using Anexia.E5E.Functions;

namespace Anexia.E5E.Exceptions;

public class E5EFunctionExecutionFailedException : E5EException
{
	public E5EContext Context { get; }
	public E5ERequest Request { get; }

	public override string Message => "The execution of the function failed.";

	public E5EFunctionExecutionFailedException(E5EContext context, E5ERequest request, Exception inner) : base(inner)
	{
		Context = context;
		Request = request;
	}
}
