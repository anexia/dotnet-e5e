using Anexia.E5E.Extensions;
using Anexia.E5E.Functions;

await Host.CreateDefaultBuilder(args)
	.UseAnexiaE5E(builder =>
	{
		builder.RegisterEntrypoint("Hello", _ =>
		{
			var res = E5EResponse.From("test");
			return Task.FromResult(res);
		});
	})
	.UseConsoleLifetime()
	.Build()
	.RunAsync();
