dotnet-e5e
===============
[![](https://img.shields.io/nuget/v/Anexia.E5E "The version badge for NuGet")](https://www.nuget.org/packages/Anexia.E5E)
[![](https://github.com/anexia/dotnet-e5e/actions/workflows/test.yml/badge.svg?branch=main "CI status")](https://github.com/anexia/dotnet-e5e/actions/workflows/test.yml)

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

```cs
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

using var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(args)
	.UseConsoleLifetime() // listen to SIGTERM and Ctrl+C, recommended by us
	.Build();

// Register our entrypoint "Hello" which just responds with "test", ignoring the request.
host.RegisterEntrypoint("Hello", _ =>
{
	var res = E5EResponse.From("test");
	return Task.FromResult(res);
});

// Finally run the host.
await host.RunAsync();
```

### Class handler

We can also use a handler that satisfies the [IE5EFunctionHandler](src/Anexia.E5E/Functions/IE5EFunctionHandler.cs).
A very simple adaption of the above handler would look like this:

```cs
using Anexia.E5E.Functions;

public class HelloHandler : IE5EFunctionHandler {
  public Task<E5EResponse> HandleAsync(E5ERequest request, CancellationToken token = default) {
    var res = E5EResponse.From("test");
  	return Task.FromResult(res);
  }
}
```

Those handlers are registered during startup as scoped. That means, that you can use dependency injection,
just like you'd do for ASP.NET Controllers!

```csharp
using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

using var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(args)
    .ConfigureServices(services => {
        services.AddFunctionHandler<HelloHandler>();
    })
	.UseConsoleLifetime() // listen to SIGTERM and Ctrl+C, recommended by us
	.Build();

// Register our entrypoint "Hello" which just responds with "test", ignoring the request.
host.RegisterEntrypoint<HelloHandler>("Hello");

// Finally run the host.
await host.RunAsync();
```

# Supported versions

|          | Supported |
|----------|-----------|
| .Net 6.0 | ✓         |
| .Net 7.0 | ✓         |
