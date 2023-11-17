using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

using var host = Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(args)
	.UseConsoleLifetime()
	.Build();

host.RegisterEntrypoint("Hello", _ =>
{
	var res = E5EResponse.From("test");
	return Task.FromResult(res);
});

await host.RunAsync();
