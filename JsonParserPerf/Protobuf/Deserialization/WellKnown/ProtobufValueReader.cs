using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct ProtobufValueReader : IValueReader<Value>
{
    public static Value Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => Value.ForNull(),
            JsonTokenType.True => Value.ForBool(true),
            JsonTokenType.False => Value.ForBool(false),
            JsonTokenType.Number => Value.ForNumber(DoubleReader.Read(ref reader, options)),
            JsonTokenType.String => Value.ForString(Utf8StringReader.ReadText(ref reader, options)),
            JsonTokenType.StartObject => Value.ForStruct(StructReader.Read(ref reader, options)),
            JsonTokenType.StartArray => new Value { ListValue = ListValueReader.Read(ref reader, options) },
            _ => throw new JsonException($"Unexpected token {reader.TokenType} while reading a Value."),
        };
    }
}