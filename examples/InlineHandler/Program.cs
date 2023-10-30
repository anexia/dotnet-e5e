using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;
using Anexia.E5E.Hosting;

using var host = E5EApplication.CreateBuilder(args)
	.UseConsoleLifetime()
	.Build();
host.RegisterEntrypoint("Hello", _ =>
{
	var res = E5EResponse.From("test");
	return Task.FromResult(res);
});

await host.RunAsync();
