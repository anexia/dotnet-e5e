using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Builders;

using VerifyTests;

using VerifyXunit;

using Xunit;

using static VerifyXunit.Verifier;

namespace Anexia.E5E.Tests.Serialization;

[UsesVerify]
public class SerializationTests
{
	private readonly JsonSerializerOptions _options;
	private readonly VerifySettings _verifySettings;

	public SerializationTests()
	{
		_options = E5EJsonSerializerOptions.Default;

		_verifySettings = new VerifySettings();
		_verifySettings.UseDirectory("snapshots");
		_verifySettings.DisableDiff();
	}

	[Theory]
	[ClassData(typeof(SerializationTestsData))]
	public void ClassCanBeSerialized(object cls) => JsonSerializer.Serialize(cls, _options);

	[Theory]
	[ClassData(typeof(RequestSerializationTestsData))]
	public Task RequestSerializationMatchesSnapshot(string testName, E5ERequest request)
	{
		var json = JsonSerializer.Serialize(request, _options);
		return Verify(json, _verifySettings).UseParameters(testName);
	}

	[Theory]
	[ClassData(typeof(ResponseSerializationTestsData))]
	public Task ResponseSerializationMatchesSnapshot(string testName, E5EResponse response)
	{
		var json = JsonSerializer.Serialize(response, _options);
		return Verify(json, _verifySettings).UseParameters(testName);
	}

	[Theory]
	[ClassData(typeof(RequestSerializationTestsData))]
	public void RequestSerializationWorksBidirectional(string _, E5ERequest input)
	{
		var json = JsonSerializer.Serialize(input, _options);
		var got = JsonSerializer.Deserialize<E5ERequest>(json, _options);
		Assert.NotNull(got);
		Assert.Multiple(
			() => Assert.Equal(input.Type, got.Type),
			() => Assert.Equal(input.RequestHeaders, got.RequestHeaders,
				E5EHttpHeaders
					.E5EHttpHeadersComparer
				!), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
			() => Assert.Equal(input.Data.GetRawText(), got.Data.GetRawText())
		);
	}


	[Theory]
	[ClassData(typeof(ResponseSerializationTestsData))]
	public void ResponseSerializationWorksBidirectional(string _, E5EResponse input)
	{
		var json = JsonSerializer.Serialize(input, _options);
		var got = JsonSerializer.Deserialize<E5EResponse>(json, _options);

		Assert.NotNull(got);
		Assert.Multiple(
			() => Assert.Equal(input.Status, got.Status),
			() => Assert.Equal(input.ResponseHeaders, got.ResponseHeaders,
				E5EHttpHeaders
					.E5EHttpHeadersComparer
				!), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
			() => Assert.Equal(input.Data.GetRawText(), got.Data.GetRawText())
		);
	}

	class SerializationTestsData : IEnumerable<object[]>
	{
		private readonly List<object> _objects = new()
		{
			new E5EContext(E5EContextType.Generic, DateTimeOffset.FromUnixTimeSeconds(0), true),
			new E5ERequestParameters(),
			new E5ERuntimeMetadata(),
		};

		private IEnumerable<object[]> Data => _objects.Select(obj => new[] { obj });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class RequestSerializationTestsData : IEnumerable<object[]>
	{
		private readonly Dictionary<string, E5ERequest> _tests = new()
		{
			{ "simple text request", E5ERequestBuilder.New("test").Build() },
			{ "simple binary request", E5ERequestBuilder.New(Encoding.UTF8.GetBytes("test")).Build() },
			{
				"simple object request",
				E5ERequestBuilder.New(new Dictionary<string, string> { { "test", "value" } }).Build()
			},
			{
				"request with headers and parameters", E5ERequestBuilder.New("test")
					.AddParam("param", "value")
					.AddHeader("Accept", "application/json")
					.Build()
			}
		};

		private IEnumerable<object[]> Data => _tests.Select(obj => new object[] { obj.Key, obj.Value });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class ResponseSerializationTestsData : IEnumerable<object[]>
	{
		private readonly Dictionary<string, E5EResponse> _tests = new()
		{
			{ "simple text response", new E5EResponse<string>("test") },
			{ "simple binary response", new E5EResponse<byte[]>(Encoding.UTF8.GetBytes("test")) },
			{
				"simple object response",
				new E5EResponse<Dictionary<string, int>>(new Dictionary<string, int> { { "a", 1 }, { "b", 2 } })
			},
			{
				"text response with headers and status code", new E5EResponse<string>("test", HttpStatusCode.Moved,
					new E5EHttpHeaders { { "Location", "https://example.com" } })
			}
		};

		private IEnumerable<object[]> Data => _tests.Select(obj => new object[] { obj.Key, obj.Value });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
