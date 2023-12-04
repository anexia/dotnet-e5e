using Anexia.E5E.Extensions;

using WithDependencyInjection;

var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(endpoints =>
	{
		endpoints.RegisterEntrypoint<Handler>("Hello");
	})
	.Build();

await host.RunAsync();
