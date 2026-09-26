using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization;

internal static class JsonToken
{
    private const NumberStyles NumberStyle =
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

    public static bool TryGetRawBytes(in Utf8JsonReader reader, out ReadOnlySpan<byte> bytes)
    {
        if (!reader.HasValueSequence && !reader.ValueIsEscaped)
        {
            bytes = reader.ValueSpan;
            return true;
        }

        bytes = default;
        return false;
    }

    public static ReadOnlySpan<byte> UnescapedUtf8(in Utf8JsonReader reader)
    {
        if (TryGetRawBytes(reader, out var bytes))
        {
            return bytes;
        }

        return Unescaped(reader);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[] Unescaped(in Utf8JsonReader reader)
    {
        return Encoding.UTF8.GetBytes(reader.GetString()!);
    }

    public static bool TryReadNumber<T>(in Utf8JsonReader reader, out T value) where T : struct, INumberBase<T>
    {
        if (reader.TokenType is JsonTokenType.Number or JsonTokenType.String or JsonTokenType.PropertyName)
        {
            return T.TryParse(UnescapedUtf8(reader), NumberStyle, CultureInfo.InvariantCulture, out value);
        }

        value = default;
        return false;
    }

    public static bool TryReadDouble(in Utf8JsonReader reader, out double value)
    {
        if (!TryReadNumber(reader, out value))
        {
            return false;
        }

        if (double.IsFinite(value))
        {
            return true;
        }

        return reader.TokenType == JsonTokenType.String
               && (reader.ValueTextEquals("NaN"u8) || reader.ValueTextEquals("Infinity"u8)
                   || reader.ValueTextEquals("-Infinity"u8));
    }

    public static void CheckDepth(in Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (reader.CurrentDepth > options.RecursionLimit)
        {
            throw new JsonException(
                $"JSON nesting goes deeper than the parser's recursion limit of {options.RecursionLimit}.");
        }
    }

    public static JsonException InvalidValue(string type)
    {
        return new JsonException($"Value is not a valid protobuf {type}.");
    }

    public static JsonException UnexpectedToken(JsonTokenType expected, JsonTokenType found, string type)
    {
        var kind = expected switch
        {
            JsonTokenType.String => "a string",
            JsonTokenType.StartObject => "an object",
            JsonTokenType.StartArray => "an array",
            _ => expected.ToString(),
        };

        return new JsonException($"Expected {kind} for {type} but found {found}.");
    }

    public static JsonException ExpectedArray(JsonTokenType found)
    {
        return new JsonException($"Expected an array for a repeated field but found {found}.");
    }

    public static JsonException ExpectedMap(JsonTokenType found)
    {
        return new JsonException($"Expected an object for a map field but found {found}.");
    }

    public static JsonException TruncatedRepeated()
    {
        return new JsonException("Unexpected end of JSON inside a repeated field.");
    }

    public static JsonException TruncatedMap()
    {
        return new JsonException("Unexpected end of JSON inside a map field.");
    }
}
