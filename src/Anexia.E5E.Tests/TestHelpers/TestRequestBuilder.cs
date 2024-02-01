using System;
using System.Collections.Generic;
using System.Text.Json;

using Anexia.E5E.Functions;

namespace Anexia.E5E.Tests.TestHelpers;

public class TestRequestBuilder
{
	private JsonElement? _data;
	private E5EHttpHeaders? _headers;
	private E5ERequestParameters? _parameters;
	private E5ERequestDataType _requestType = E5ERequestDataType.StructuredObject;

	public TestRequestBuilder WithData<T>(T data)
	{
		_requestType = data switch
		{
			string => E5ERequestDataType.Text,
			IEnumerable<byte> => throw new InvalidOperationException(
				$"E5E does not compose binary requests just from the bytes. Please convert this call to use {nameof(E5EFileData)} instead."),
			E5EFileData => E5ERequestDataType.Binary,
			_ => E5ERequestDataType.StructuredObject,
		};
		_data = JsonSerializer.SerializeToElement(data);
		return this;
	}

	public E5EEvent BuildEvent()
	{
		return new E5EEvent(_requestType, _data, _headers, _parameters);
	}

	public TestRequestBuilder AddHeader(string key, string value)
	{
		_headers ??= new E5EHttpHeaders();
		_headers.Add(key, value);
		return this;
	}

	public TestRequestBuilder AddParam(string key, string value)
	{
		_parameters ??= new E5ERequestParameters();
		_parameters.Add(key, new List<string> { value });
		return this;
	}
}
