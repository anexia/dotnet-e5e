using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Anexia.E5E.Abstractions;
using Anexia.E5E.Exceptions;
using Anexia.E5E.Extensions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Fixtures;
using Anexia.E5E.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Anexia.E5E.Tests.Extensions;

public class HostApplicationBuilderExtensionsTests
{
	private readonly ITestOutputHelper _outputHelper;

	public HostApplicationBuilderExtensionsTests(ITestOutputHelper outputHelper)
	{
		_outputHelper = outputHelper;
	}

	[Fact]
	public void EmptyListOfArgumentsThrowsException()
	{
		Assert.Throws<E5EMissingArgumentsException>(() => Host.CreateDefaultBuilder().UseAnexiaE5E(Array.Empty<string>()).Build());
	}

	[Fact]
	public void IncorrectListOfArgumentsThrowsException()
	{
		Assert.Throws<E5EMissingArgumentsException>(() =>
			Host.CreateDefaultBuilder().UseAnexiaE5E(new[] { "foo", "bar" }).Build());
	}

	[Fact]
	public async Task MetadataIsReturned()
	{
		var abstraction = new TestConsoleAbstraction(new NullLogger<TestConsoleAbstraction>());
		var host = Host.CreateDefaultBuilder()
			.UseAnexiaE5E(new[] { "metadata" })
			.ConfigureServices(services => services.AddSingleton<IConsoleAbstraction>(abstraction))
			.Build();

		// Cancel after three seconds if the metadata is not returned.
		// This indicates an error in the code.
		var sw = Stopwatch.StartNew();
		await host.RunAsync(new CancellationTokenSource(3000).Token);
		sw.Stop();

		var expected = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
		Assert.Equal(expected, abstraction.Stdout());
		Assert.InRange(sw.ElapsedMilliseconds, 0, 3000); // Shutdown should be triggered properly without the timeout
	}

	[Fact]
	public void ShouldReadEscapedNullBytes()
	{
		var host = Host.CreateDefaultBuilder().UseAnexiaE5E(new[] { "entrypoint", "\\0", "1", "\\0" }).Build();
		var got = host.Services.GetRequiredService<E5ERuntimeOptions>();
		var expected = new E5ERuntimeOptions("entrypoint", "\0", "\0", true);

		Assert.Equal(expected, got);
	}
}
