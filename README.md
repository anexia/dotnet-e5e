dotnet-e5e
==========
[![](https://img.shields.io/nuget/v/Anexia.E5E "NuGet version badge")](https://www.nuget.org/packages/Anexia.E5E)
[![](https://github.com/anexia/dotnet-e5e/actions/workflows/test.yml/badge.svg?branch=main "Test status")](https://github.com/anexia/dotnet-e5e/actions/workflows/test.yml)

`dotnet-e5e` is a client library for Anexia e5e - our *Functions as a Service* offering.
With our client library, it's easy to build functions that can scale indefinitely!

# Install

With a correctly set up .NET SDK:

```shell
dotnet add package Anexia.E5E
```

# Getting started

## Creating our application

`Anexia.E5E` is built on top of the `Microsoft.Extensions.Hosting.IHost`, so we need to create
a new *Worker application*. This can be easily done on the command line by invoking:

```shell
dotnet new worker --name MyNewFunctionHandler
cd MyNewFunctionHandler
dotnet add package Anexia.E5E
```

### Inline handler

With that, we have a `Program.cs` that we can modify to use our library:

```csharp
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

using var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(builder =>
	{
		// Register our entrypoint "Hello" which just responds with the name of the person.
		builder.RegisterEntrypoint("Hello", request =>
		{
			var (evt, context) = request;
			// Let's assume we got the name as a plain text message.
			var name = evt.AsText();
			var res = E5EResponse.From($"Hello {name}");
			return Task.FromResult(res);
		});
	})
	.UseConsoleLifetime() // listen to SIGTERM and Ctrl+C, recommended by us
	.Build();

// Finally run the host.
await host.RunAsync();
```

### Class handler

We can also use a handler that satisfies the [IE5EFunctionHandler](src/Anexia.E5E/Functions/IE5EFunctionHandler.cs).
A very simple adaption of the above handler would look like this:

```csharp
using Anexia.E5E.Functions;

public class HelloHandler : IE5EFunctionHandler {
	private readonly ILogger<HelloHandler> _logger; 
	public HelloHandler(ILogger<HelloHandler> logger) {
		_logger = logger;
	}
	
	public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken token = default) {
		_logger.LogDebug("Received a {Request}", request);
		
		var (evt, context) = request;
		// Let's assume we got the name as a plain text message.
		var name = evt.AsText();
		var res = E5EResponse.From($"Hello {name}");
		return Task.FromResult(res);
	}
}
```

Those handlers are automatically registered as scoped. That means, that you can use dependency injection,
just like you'd do for ASP.NET Controllers!

```csharp
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

using var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(builder =>
	{
		builder.RegisterEntrypoint<HelloHandler>("Hello");
	})
	.UseConsoleLifetime() // listen to SIGTERM and Ctrl+C, recommended by us
	.Build();
await host.RunAsync();
```

Further examples can be found in the [examples folder](./examples).

# Supported versions

|                        | Supported |
|------------------------|-----------|
| .NET 6.0               | ✓         |
| .NET 8.0 (without AOT) | ✓         |
| .NET 8.0 (with AOT)    | ✓         |
