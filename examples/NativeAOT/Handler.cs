using System.Text.Json;

using Anexia.E5E.Functions;

namespace NativeAOT;
public record HelloRequest(string FirstName, string LastName);
public class Handler : IE5EFunctionHandler
{
	public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken cancellationToken = default)
	{
		// The custom serialization context is required in order to use AOT-compatible (de-)serialization.
		var (firstName, lastName) = request.Event.As(AotSerializationContext.Default.HelloRequest)!;
		return Task.FromResult(new E5EResponse
		{
			Data = JsonSerializer.SerializeToElement($"Hi {firstName}! Your last name is: {lastName}",
				AotSerializationContext.Default.String),
			Type = E5EResponseType.Text,
		});
	}
}
