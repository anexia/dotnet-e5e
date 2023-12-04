using Anexia.E5E.Extensions;

var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(endpoints =>
	{
		endpoints.RegisterEntrypoint<NativeAOT.Handler>("Hello");
	})
	.Build();

await host.RunAsync();
