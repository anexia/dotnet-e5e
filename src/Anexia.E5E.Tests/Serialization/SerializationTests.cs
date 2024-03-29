using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Runtime;
using Anexia.E5E.Serialization;
using Anexia.E5E.Tests.Helpers;
using Anexia.E5E.Tests.TestHelpers;

using VerifyTests;

using VerifyXunit;

using Xunit;

using static VerifyXunit.Verifier;

namespace Anexia.E5E.Tests.Serialization;

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
	public void ClassCanBeSerialized(object cls)
	{
		JsonSerializer.Serialize(cls, _options);
	}

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
				E5EHttpHeadersEqualityComparer
					.Instance), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
			() => Assert.Equal(input.Data.GetValueOrDefault().GetRawText(), got.Data.GetValueOrDefault().GetRawText())
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
				E5EHttpHeadersEqualityComparer
					.Instance), // todo: remove custom comparer once https://github.com/xunit/xunit/issues/2803 is closed
			() => Assert.Equal(input.Data.GetRawText(), got.Data.GetRawText())
		);
	}

	[Fact]
	public void ResponseSerializationRecognisesCorrectType()
	{
		Assert.Equal(E5EResponseType.Text, E5EResponse.From("test").Type);
		Assert.Equal(E5EResponseType.Binary, E5EResponse.From("test"u8.ToArray()).Type);
		Assert.Equal(E5EResponseType.Binary, E5EResponse.From("test"u8.ToArray().AsEnumerable()).Type);
		Assert.Equal(E5EResponseType.Binary, E5EResponse.From(new E5EFileData("something"u8.ToArray())).Type);
		Assert.Equal(E5EResponseType.StructuredObject, E5EResponse.From(E5ERuntimeMetadata.Current).Type);
	}

	[Theory]
	[InlineData(E5ERequestDataType.Text, "text")]
	[InlineData(E5ERequestDataType.Binary, "binary")]
	[InlineData(E5ERequestDataType.StructuredObject, "object")]
	[InlineData(E5ERequestDataType.Mixed, "mixed")]
	[InlineData(E5EResponseType.Text, "text")]
	[InlineData(E5EResponseType.Binary, "binary")]
	[InlineData(E5EResponseType.StructuredObject, "object")]
	public void EnumsAreProperSerialized(object type, string expected)
	{
		// serialization
		var json = JsonSerializer.Serialize(type, _options);
		Assert.Equal($"\"{expected}\"", json);

		// deserialization
		var deserialized = JsonSerializer.Deserialize(json, type.GetType(), _options);
		Assert.Equal(type, deserialized);
	}

	[Fact]
	public void MetadataIsProperSerialized()
	{
		var json = JsonSerializer.Serialize(E5ERuntimeMetadata.Current, _options);
		var deserialized = JsonSerializer.Deserialize<JsonElement>(json);
		var sut = deserialized.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);

		Assert.Multiple(
			() => Assert.Equal(4, sut.Count),
			() => Assert.Contains("runtime_version", sut),
			() => Assert.Contains("runtime", sut),
			() => Assert.Contains("features", sut),
			() => Assert.Contains("library_version", sut),
			() => Assert.Equal(1, sut["features"].GetArrayLength()),
			() => Assert.Equal("1.0.0", sut["library_version"].GetString())
		);
	}

	[Fact]
	public void DataIsNotAffectedByPropertyNamingPolicy()
	{
		var json = JsonSerializer.Serialize(E5EResponse.From(new DataIsNotAffectedByPropertyNamingPolicyTest()),
			_options);
		Assert.Equal(@"{""data"":{""MyProperty"":""value""},""type"":""object""}", json);
	}

	private class DataIsNotAffectedByPropertyNamingPolicyTest
	{
		public string MyProperty { get; set; } = "value";
	}

	private class SerializationTestsData : IEnumerable<object[]>
	{
		private readonly List<object> _objects = new()
		{
			new E5EContext("generic", DateTimeOffset.FromUnixTimeSeconds(0), true),
			new E5ERequestParameters(),
			E5ERuntimeMetadata.Current,
			new E5EFileData("data"u8.ToArray()),
		};

		private IEnumerable<object[]> Data => _objects.Select(obj => new[] { obj });

		public IEnumerator<object[]> GetEnumerator()
		{
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class RequestSerializationTestsData : IEnumerable<object[]>
	{
		private readonly Dictionary<string, E5EEvent> _tests = new()
		{
			{ "simple text request", new TestRequestBuilder().WithData("test").BuildEvent() },
			{ "simple binary request", new TestRequestBuilder().WithData(new E5EFileData("hello"u8.ToArray())).BuildEvent() },
			{ "simple object request", new TestRequestBuilder().WithData(new Dictionary<string, string> { { "test", "value" } }).BuildEvent() },
			{
				"request with headers and parameters", new TestRequestBuilder().WithData("test")
					.AddParam("param", "value")
					.AddHeader("Accept", "application/json")
					.BuildEvent()
			},
		};

		private IEnumerable<object[]> Data => _tests.Select(obj => new object[] { obj.Key, obj.Value });

		public IEnumerator<object[]> GetEnumerator()
		{
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class ResponseSerializationTestsData : IEnumerable<object[]>
	{
		private readonly Dictionary<string, E5EResponse> _tests = new()
		{
			{ "simple text response", E5EResponse.From("test") },
			{ "simple binary response", E5EResponse.From("hello"u8.ToArray()) },
			{
				"simple object response", E5EResponse.From(new Dictionary<string, int>
				{
					{ "a", 1 },
					{ "b", 2 },
				})
			},
			{ "text response with headers and status code", E5EResponse.From("test", HttpStatusCode.Moved, new E5EHttpHeaders { { "Location", "https://example.com" } }) },
		};

		private IEnumerable<object[]> Data => _tests.Select(obj => new object[] { obj.Key, obj.Value });

		public IEnumerator<object[]> GetEnumerator()
		{
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
