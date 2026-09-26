using System.Text.Json;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal interface IValueReader<TValue>
{
    static virtual JsonTokenType Token => JsonTokenType.None;

    static abstract TValue Read(ref Utf8JsonReader reader, ProtobufParserOptions options);
}
