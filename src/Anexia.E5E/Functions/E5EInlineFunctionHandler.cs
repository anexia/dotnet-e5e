namespace Anexia.E5E.Functions;

internal class E5EInlineFunctionHandler : IE5EFunctionHandler
{
	private readonly Func<E5ERequest, CancellationToken, Task<E5EResponse>> _func;

	public string Name { get; }

	public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default) =>
		_func.Invoke(request, cancellationToken);

	public E5EInlineFunctionHandler(string name, Func<E5ERequest, CancellationToken, Task<E5EResponse>> func)
	{
		Name = name;
		_func = func;
	}
}
