namespace Anexia.E5E.Functions;

internal class E5EInlineFunctionHandler : IE5EFunctionHandler
{
	private readonly Func<E5ERequest, Task<E5EResponse>> _func;

	public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default) =>
		_func.Invoke(request);

	public E5EInlineFunctionHandler(Func<E5ERequest, Task<E5EResponse>> func) => _func = func;
}
