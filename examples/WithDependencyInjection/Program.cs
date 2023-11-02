using Anexia.E5E.Extensions;

using WithDependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services => services.AddFunctionHandler<Handler>())
	.Build();

host.RegisterEntrypoint<Handler>("Hello");
await host.RunAsync();
