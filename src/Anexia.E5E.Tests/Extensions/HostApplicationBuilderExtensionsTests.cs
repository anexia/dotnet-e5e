using System;
using System.IO;
using System.Text.Json;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Hosting;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Anexia.E5E.Tests.Extensions;

public class HostApplicationBuilderExtensionsTests
{
	[Fact]
	public void EmptyListOfArgumentsThrowsException()
	{
		Assert.Throws<E5EMissingArgumentsException>(() => E5EApplication.CreateBuilder(Array.Empty<string>()).Build());
	}

	[Fact]
	public void IncorrectListOfArgumentsThrowsException()
	{
		Assert.Throws<E5EMissingArgumentsException>(() =>
			E5EApplication.CreateBuilder(new[] { "foo", "bar" }).Build());
	}

	[Fact]
	public void MetadataIsReturned()
	{
		using var _ = Console.Out;
		using var sw = new StringWriter();
		Console.SetOut(sw);

		var expected = JsonSerializer.Serialize(new E5ERuntimeMetadata(), E5EJsonSerializerOptions.Default);
		E5EApplication.CreateBuilder(new[] { "metadata" }).Build().Run();

		Assert.Equal(expected, sw.ToString());
	}

	[Fact]
	public void ShouldReadEscapedNullBytes()
	{
		var host = E5EApplication.CreateBuilder(new[] { "entrypoint", "\\0", "1", "\\0" }).Build();
		var got = host.Services.GetRequiredService<E5ERuntimeOptions>();
		var expected = new E5ERuntimeOptions("entrypoint", "\0", "\0", true);

		Assert.Equal(expected, got);
	}
}
