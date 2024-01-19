using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using Xunit;
using Xunit.Abstractions;

namespace Anexia.E5E.Tests.Integration;

/// <summary>
/// Those are the examples from the public documentation. If the code in here changes extensively, update the documentation as well!
/// </summary>
public class DocumentationTests : IntegrationTestBase
{
	public DocumentationTests(ITestOutputHelper outputHelper) : base(outputHelper)
	{
	}

	[Fact]
	public async Task SimpleHandler()
	{
		await Host.StartWithTestEntrypointAsync(_ => E5EResponse.From("test"));
		var response = await Host.WriteExampleRequestOnceAsync();
		Assert.Equal("test", response.Text());
	}

	class SumData
	{
		public int A { get; set; }
		public int B { get; set; }
	}

	[Fact]
	public async Task Sum()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			var data = request.Event.As<SumData>()!;
			return E5EResponse.From(data.A + data.B);
		});
		var response = await Host.WriteRequestOnceAsync(x => x.WithData(new SumData
		{
			A = 3,
			B = 2,
		}));
		Assert.Equal(5, response.As<int>());
	}

	[Fact]
	public async Task WorkingWithBinaryData()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			var name = Encoding.UTF8.GetString(request.Event.AsBytes()!);
			var resp = Encoding.UTF8.GetBytes($"Hello {name}");
			return E5EResponse.From(resp);
		});
		var response = await Host.WriteRequestOnceAsync(x => x.WithData(new E5EFileData("Luna"u8.ToArray())));
		Assert.Equal("Hello Luna"u8.ToArray(), response.Data.Deserialize<E5EFileData>()!.Bytes);
		Assert.Equal(E5EResponseType.Binary, response.Type);
	}

	[Fact]
	public async Task WorkingWithHttpHeaders()
	{
		await Host.StartWithTestEntrypointAsync(request =>
		{
			request.Event.RequestHeaders!.TryGetValue("content-type", out var header);
			var responseHeaders = new E5EHttpHeaders { { "x-sent-content-type", header } };
			return E5EResponse.From("Hello world!", responseHeaders: responseHeaders);
		});
		var response = await Host.WriteRequestOnceAsync(x => x.AddHeader("content-type", "application/json"));
		var hasHeader = response.ResponseHeaders!.TryGetValue("x-sent-content-type", out var header);
		Assert.True(hasHeader);
		Assert.Equal("application/json", header);
	}
}
