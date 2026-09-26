using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct EmptyReader : IValueReader<Empty>
{
    public static JsonTokenType Token => JsonTokenType.StartObject;

    public static Empty Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        JsonToken.CheckDepth(reader, options);
        reader.Skip();
        return new Empty();
    }
}
