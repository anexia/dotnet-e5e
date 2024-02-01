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
		builder.RegisterEntrypoint("Binary", request =>
		{
			// This entrypoint receives one file (type: binary) and returns an anonymous object with the length.
			var fileData = request.Event.AsBytes();
			return Task.FromResult(E5EResponse.From(new { FileLength = fileData?.LongLength }));
		});
		builder.RegisterEntrypoint("ReturnFirstFile", request =>
		{
			// This entrypoint receives multiple files as a mixed request and returns the first.
			var files = request.Event.AsFiles();
			return Task.FromResult(E5EResponse.From(files.First()));
		});
	})
	.UseConsoleLifetime()
	.Build()
	.RunAsync();
