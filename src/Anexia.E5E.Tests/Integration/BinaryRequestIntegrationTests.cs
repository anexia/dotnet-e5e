using System.Threading.Tasks;

using Anexia.E5E.Exceptions;
using Anexia.E5E.Functions;
using Anexia.E5E.Tests.TestHelpers;

using static Anexia.E5E.Tests.TestData.TestData;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

public sealed class BinaryRequestIntegrationTests(ITestOutputHelper outputHelper) : IntegrationTestBase(outputHelper)
{
	[Fact]
	public async Task DecodeToBytesThrowsForMixedRequest()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			Assert.Throws<E5EInvalidConversionException>(() => request.Event.AsBytes());
			return null!;
		});
		await Host.WriteOnceAsync(BinaryRequestWithMultipleFiles);
	}

	[Fact]
	public async Task MultipleFilesAreProperlyDecoded()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			var content = "Hello world!"u8.ToArray();
			var files = request.Event.AsFiles();
			Assert.Collection(files, first =>
			{
				Assert.NotNull(first);
				Assert.Equivalent(first, new E5EFileData(content)
				{
					FileSizeInBytes = 12,
					Filename = "my-file-1.name",
					ContentType = "application/my-content-type-1",
					Charset = "utf-8",
				});
			}, second =>
			{
				Assert.NotNull(second);
				Assert.Equivalent(second, new E5EFileData(content)
				{
					FileSizeInBytes = 12,
					Filename = "my-file-2.name",
					ContentType = "application/my-content-type-2",
					Charset = "utf-8",
				});
			});
			return null!;
		});
		await Host.WriteOnceAsync(BinaryRequestWithMultipleFiles);
	}

	[Fact]
	public async Task UnknownContentType()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			Assert.Equal("Hello world!"u8.ToArray(), request.Event.AsBytes());
			return null!;
		});
		await Host.WriteOnceAsync(BinaryRequestWithUnknownContentType);
	}

	[Fact]
	public async Task FallbackForByteArrayReturnsValidResponse()
	{
		// act
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From("Hello world!"u8.ToArray()));
		var response = await Host.WriteOnceAsync(x => x.WithData("test"));

		// assert
		const string expected =
			"""
			{"data":{"binary":"SGVsbG8gd29ybGQh","type":"binary","size":0,"name":"dotnet-e5e-binary-response.blob","content_type":"application/octet-stream","charset":"utf-8"},"type":"binary"}
			""";
		Assert.Contains(expected, response.Stdout);
	}

	[Fact]
	public async Task FileDataReturnsValidResponse()
	{
		// act
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From(new E5EFileData("Hello world!"u8.ToArray())
		{
			Type = "binary",
			FileSizeInBytes = 16,
			Filename = "hello-world.txt",
			ContentType = "text/plain",
			Charset = "utf-8",
		}));
		var response = await Host.WriteOnceAsync(x => x.WithData("test"));

		// assert
		const string expected =
			"""
			{"data":{"binary":"SGVsbG8gd29ybGQh","type":"binary","size":16,"name":"hello-world.txt","content_type":"text/plain","charset":"utf-8"},"type":"binary"}
			""";
		Assert.Contains(expected, response.Stdout);
	}
}
