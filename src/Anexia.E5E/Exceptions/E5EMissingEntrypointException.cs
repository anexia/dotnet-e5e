namespace Anexia.E5E.Exceptions;

public class E5EMissingEntrypointException : E5EException
{
	public E5EMissingEntrypointException(string entrypoint) : base(
		"There is no function registered for the entrypoint: " + entrypoint)
	{
	}
}
