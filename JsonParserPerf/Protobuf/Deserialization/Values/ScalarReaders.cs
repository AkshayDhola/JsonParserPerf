using System.Text;
using System.Text.Json;
using Google.Protobuf;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal struct Int32Reader : IValueReader<int>
{
    public static int Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadNumber(reader, out int value))
        {
            throw JsonToken.InvalidValue("int32");
        }

        return value;
    }
}

internal struct Int64Reader : IValueReader<long>
{
    public static long Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadNumber(reader, out long value))
        {
            throw JsonToken.InvalidValue("int64");
        }

        return value;
    }
}

internal struct UInt32Reader : IValueReader<uint>
{
    public static uint Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadNumber(reader, out uint value))
        {
            throw JsonToken.InvalidValue("uint32");
        }

        return value;
    }
}

internal struct UInt64Reader : IValueReader<ulong>
{
    public static ulong Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadNumber(reader, out ulong value))
        {
            throw JsonToken.InvalidValue("uint64");
        }

        return value;
    }
}

internal struct DoubleReader : IValueReader<double>
{
    public static double Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadDouble(reader, out var value))
        {
            throw JsonToken.InvalidValue("double");
        }

        return value;
    }
}

internal struct FloatReader : IValueReader<float>
{
    public static float Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryReadDouble(reader, out var value))
        {
            throw JsonToken.InvalidValue("float");
        }

        var result = (float)value;

        // A finite double too large for a float is out of range, not infinity.
        if (float.IsInfinity(result) && !double.IsInfinity(value))
        {
            throw JsonToken.InvalidValue("float");
        }

        return result;
    }
}

internal struct BoolReader : IValueReader<bool>
{
    public static bool Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => throw JsonToken.InvalidValue("bool"),
        };
    }
}

internal struct Utf8StringReader : IValueReader<string>
{
    public static JsonTokenType Token => JsonTokenType.String;

    public static string Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return ReadText(ref reader, options);
    }

    public static string ReadText(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!JsonToken.TryGetRawBytes(reader, out var utf8))
        {
            return reader.GetString()!;
        }

        if (options.StringCache is not { } cache)
        {
            return Decode(ref reader, utf8);
        }

        if (cache.TryGet(utf8, out var value, out var hash))
        {
            return value;
        }

        value = Decode(ref reader, utf8);
        cache.Add(utf8, hash, value);
        return value;
    }

    private static string Decode(ref Utf8JsonReader reader, scoped ReadOnlySpan<byte> utf8)
    {
        if (Ascii.IsValid(utf8))
        {
            return Encoding.Latin1.GetString(utf8);
        }

        return reader.GetString()!;
    }
}

internal struct BytesReader : IValueReader<ByteString>
{
    public static JsonTokenType Token => JsonTokenType.String;

    public static ByteString Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        var raw = JsonToken.TryGetRawBytes(reader, out var base64);
        if (raw && base64.IsEmpty)
        {
            return ByteString.Empty;
        }

        // A ByteString is immutable, so a repeated blob can hand back the one already decoded. Only an unescaped
        // token is looked up, since its bytes are the key.
        Utf8Cache<ByteString>? cache = null;
        if (raw)
        {
            cache = options.BytesCache;
        }

        var hash = 0UL;
        if (cache is not null && cache.TryGet(base64, out var cached, out hash))
        {
            return cached;
        }

        ByteString bytes;
        try
        {
            // The array is freshly allocated and never shared, so wrapping it without a copy is safe.
            bytes = UnsafeByteOperations.UnsafeWrap(reader.GetBytesFromBase64());
        }
        catch (FormatException ex)
        {
            throw new JsonException("Value is not valid base64 for a bytes field.", ex);
        }

        cache?.Add(base64, hash, bytes);
        return bytes;
    }
}
