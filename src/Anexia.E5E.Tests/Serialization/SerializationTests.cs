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
using Anexia.E5E.Tests.Helpers;

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
	public Task RequestSerializationMatchesSnapshot(string testName, E5EEvent evt)
	{
		var json = JsonSerializer.Serialize(evt, _options);
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
	public void RequestSerializationWorksBidirectional(string _, E5EEvent input)
	{
		var json = JsonSerializer.Serialize(input, _options);
		var got = JsonSerializer.Deserialize<E5EEvent>(json, _options);
		Assert.NotNull(got);
		Assert.Multiple(
			() => Assert.Equal(input.Type, got.Type),
			() => Assert.Equal(input.RequestHeaders, got.RequestHeaders,
				E5EHttpHeadersEqualityComparer.Instance), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
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
				E5EHttpHeadersEqualityComparer.Instance), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
			() => Assert.Equal(input.Data.GetRawText(), got.Data.GetRawText())
		);
	}

	[Fact]
	public void ResponseSerializationRecognisesCorrectType()
	{
		Assert.Equal(E5EResponseType.Text, E5EResponse.From("test").Type);
		Assert.Equal(E5EResponseType.Binary, E5EResponse.From(Encoding.UTF8.GetBytes("test")).Type);
		Assert.Equal(E5EResponseType.Binary, E5EResponse.From(Encoding.UTF8.GetBytes("test").AsEnumerable()).Type);
		Assert.Equal(E5EResponseType.StructuredObject, E5EResponse.From(new E5ERuntimeMetadata()).Type);
	}

	[Theory]
	[InlineData(E5ERequestDataType.Text, "text")]
	[InlineData(E5ERequestDataType.Binary, "binary")]
	[InlineData(E5ERequestDataType.StructuredObject, "object")]
	[InlineData(E5ERequestDataType.Mixed, "mixed")]
	[InlineData(E5EResponseType.Text, "text")]
	[InlineData(E5EResponseType.Binary, "binary")]
	[InlineData(E5EResponseType.StructuredObject, "object")]
	public void EnumsAreProperSerialized(Enum type, string expected)
	{
		Assert.Equal(expected, JsonSerializer.Serialize(type, _options));
	}

	class SerializationTestsData : IEnumerable<object[]>
	{
		private readonly List<object> _objects = new()
		{
			new E5ERequestContext("generic", DateTimeOffset.FromUnixTimeSeconds(0), true),
			new E5ERequestParameters(),
			new E5ERuntimeMetadata(),
		};

		private IEnumerable<object[]> Data => _objects.Select(obj => new[] { obj });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class RequestSerializationTestsData : IEnumerable<object[]>
	{
		private readonly Dictionary<string, E5EEvent> _tests = new()
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
			{ "simple text response", E5EResponse.From("test") },
			{ "simple binary response", E5EResponse.From(Encoding.UTF8.GetBytes("test")) },
			{
				"simple object response",
				E5EResponse.From(new Dictionary<string, int> { { "a", 1 }, { "b", 2 } })
			},
			{
				"text response with headers and status code",E5EResponse.From("test", HttpStatusCode.Moved,
					new E5EHttpHeaders { { "Location", "https://example.com" } })
			}
		};

		private IEnumerable<object[]> Data => _tests.Select(obj => new object[] { obj.Key, obj.Value });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
