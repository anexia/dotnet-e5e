using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Anexia.E5E.Functions;
using Anexia.E5E.Tests.TestHelpers;

using Microsoft.Extensions.Hosting;

namespace Anexia.E5E.Tests.Builders;

public interface IE5ERequestBuilder
{
	E5EEvent Build();

	// TODO: refactor into HostFixture itself
	Task SendAndShutdownAsync(IHost host)
	{
		return host.WriteToStdinOnceAsync(Build());
	}

	IE5ERequestBuilder AddHeader(string key, string value);
	IE5ERequestBuilder AddParam(string key, string value);
}

public static class E5ERequestBuilder
{
	public static IE5ERequestBuilder New<T>(T data)
	{
		return new E5ERequestBuilderInner(data ?? throw new ArgumentNullException(nameof(data)));
	}

	private class E5ERequestBuilderInner : IE5ERequestBuilder
	{
		private readonly JsonElement _data;
		private readonly E5ERequestDataType? _requestType;
		private E5EHttpHeaders? _headers;
		private E5ERequestParameters? _parameters;

		public E5ERequestBuilderInner(object data)
		{
			_requestType = data switch
			{
				string => E5ERequestDataType.Text,
				IEnumerable<byte> => E5ERequestDataType.Binary,
				_ => E5ERequestDataType.StructuredObject,
			};

			_data = JsonSerializer.SerializeToElement(data);
		}

		public E5EEvent Build()
		{
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
