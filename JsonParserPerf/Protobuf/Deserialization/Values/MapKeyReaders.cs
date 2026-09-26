using System.Text.Json;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal struct StringKeyReader : IValueReader<string>
{
    public static string Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return Utf8StringReader.ReadText(ref reader, options);
    }
}

internal struct BoolKeyReader : IValueReader<bool>
{
    public static bool Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (reader.ValueTextEquals("true"u8))
        {
            return true;
        }

        if (reader.ValueTextEquals("false"u8))
        {
            return false;
        }

        throw new JsonException("Map key is not a valid bool.");
    }
}
