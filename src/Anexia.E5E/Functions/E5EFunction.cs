namespace Anexia.E5E.Functions;

public interface IE5EFunction
{
	string Name { get; }
	Task<E5EResponse> RunAsync(E5ERequest request, CancellationToken cancellationToken = default);
}
