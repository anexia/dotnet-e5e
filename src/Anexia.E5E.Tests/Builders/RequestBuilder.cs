using System;
using System.Collections.Generic;
using System.Text.Json;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.Fixtures;

namespace Anexia.E5E.Tests.Builders;

public interface IE5ERequestBuilder
{
	E5EEvent Build();
	void SendTo(HostFixture host) { host.WriteToStdinAndClose(Build()); }
	IE5ERequestBuilder AddHeader(string key, string value);
	IE5ERequestBuilder AddParam(string key, string value);
}

public static class E5ERequestBuilder
{
	public static IE5ERequestBuilder New<T>(T data) =>
		new E5ERequestBuilderInner(data ?? throw new ArgumentNullException(nameof(data)));

	private class E5ERequestBuilderInner : IE5ERequestBuilder
	{
		private E5ERequestParameters? _parameters;
		private E5EHttpHeaders? _headers;
		private readonly JsonElement _data;
		private readonly E5ERequestDataType? _requestType;

		public E5ERequestBuilderInner(object data)
		{
			_requestType = data switch
			{
				string => E5ERequestDataType.Text,
				IEnumerable<byte> => E5ERequestDataType.Binary,
				_ => E5ERequestDataType.StructuredObject
			};

			_data = JsonSerializer.SerializeToElement(data);
		}

		public E5EEvent Build()
		{
			if (_requestType is null) throw new ArgumentNullException(nameof(_requestType));

			return new E5EEvent(_requestType.GetValueOrDefault(), _data, _headers, _parameters);
		}

		public IE5ERequestBuilder AddHeader(string key, string value)
		{
			_headers ??= new E5EHttpHeaders();
			_headers.Add(key, value);
			return this;
		}

		public IE5ERequestBuilder AddParam(string key, string value)
		{
			_parameters ??= new E5ERequestParameters();
			_parameters.Add(key, new List<string> { value });
			return this;
		}
	}
}
