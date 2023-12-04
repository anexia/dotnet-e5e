namespace Anexia.E5E.Functions;

internal sealed class E5EInlineFunctionHandler : IE5EFunctionHandler
{
	private readonly Func<E5ERequest, Task<E5EResponse>> _func;

	public E5EInlineFunctionHandler(Func<E5ERequest, Task<E5EResponse>> func)
	{
		_func = func;
	}

	public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
	{
		return _func.Invoke(request);
	}
}
