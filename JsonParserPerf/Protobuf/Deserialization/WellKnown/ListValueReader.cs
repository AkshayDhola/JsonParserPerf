using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct ListValueReader : IValueReader<ListValue>
{
    public static JsonTokenType Token => JsonTokenType.StartArray;

    public static ListValue Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        JsonToken.CheckDepth(reader, options);
        var result = new ListValue();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return result;
            }

            result.Values.Add(ProtobufValueReader.Read(ref reader, options));
        }

        throw StructReader.Truncated();
    }
}
