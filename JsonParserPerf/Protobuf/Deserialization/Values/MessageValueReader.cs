using System.Text.Json;
using Google.Protobuf;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal struct MessageValueReader<T> : IValueReader<T>
    where T : class, IMessage<T>, new()
{
    public static T Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return ProtobufJsonParser<T>.Instance.ParseObject(ref reader, options);
    }
}
