using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct StructReader : IValueReader<Struct>
{
    public static JsonTokenType Token => JsonTokenType.StartObject;

    public static Struct Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        // Arbitrary JSON is where nesting can run away.
        JsonToken.CheckDepth(reader, options);
        var result = new Struct();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            var name = Utf8StringReader.ReadText(ref reader, options);
            reader.Read();
            result.Fields[name] = ProtobufValueReader.Read(ref reader, options);
        }

        throw Truncated();
    }

    internal static JsonException Truncated()
    {
        return new JsonException("Unexpected end of JSON input.");
    }
}