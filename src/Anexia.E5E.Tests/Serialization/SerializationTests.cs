using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Anexia.E5E.Functions;

using Xunit;

namespace Anexia.E5E.Tests.Serialization;

public class SerializationTests
{
	[Theory]
	[ClassData(typeof(SerializationTestsData))]
	public void ClassCanBeSerialized(object cls) => JsonSerializer.Serialize(cls);

	[Theory]
	[ClassData(typeof(SerializationTestsData))]
	public void ClassCanBeDeserialized(object cls) => JsonSerializer.Deserialize("{}", cls.GetType());

	class SerializationTestsData : IEnumerable<object[]>
	{
		private readonly List<object> _objects = new()
		{
			new E5EContext(E5EContextType.Generic, DateTimeOffset.Now, true),
			new E5ERequest(),
			E5ERequest.FromString("test"),
			new E5ERequestParameters(),
			new E5EResponse(),
		};

		private IEnumerable<object[]> Data => _objects.Select(obj => new[] { obj });
		public IEnumerator<object[]> GetEnumerator() => Data.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
