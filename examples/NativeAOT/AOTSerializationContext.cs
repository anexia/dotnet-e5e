using System.Text.Json.Serialization;

namespace NativeAOT;

[JsonSerializable(typeof(HelloRequest))]
[JsonSerializable(typeof(string))]
internal sealed partial class AotSerializationContext : JsonSerializerContext;
