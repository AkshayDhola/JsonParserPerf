using System.Globalization;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct DurationReader : IValueReader<Duration>
{
    private const long MaxSeconds = 315_576_000_000L;

    public static JsonTokenType Token => JsonTokenType.String;

    public static Duration Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        var utf8 = JsonToken.UnescapedUtf8(reader);
        var negative = utf8.StartsWith("-"u8);
        var body = utf8;
        if (negative)
        {
            body = utf8[1..];
        }

        var nanos = 0;

        if (!body.EndsWith("s"u8))
        {
            throw Invalid();
        }

        body = body[..^1];
        var dot = body.IndexOf((byte)'.');
        var whole = body;
        if (dot >= 0)
        {
            whole = body[..dot];
        }

        if (!long.TryParse(whole, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            || (dot >= 0 && !TryParseNanos(body[(dot + 1)..], out nanos))
            || seconds > MaxSeconds || (seconds == MaxSeconds && nanos != 0))
        {
            throw Invalid();
        }

        if (negative)
        {
            seconds = -seconds;
            nanos = -nanos;
        }

        return new Duration { Seconds = seconds, Nanos = nanos };
    }

    internal static bool TryParseNanos(ReadOnlySpan<byte> digits, out int nanos)
    {
        nanos = 0;
        if (digits.Length is 0 or > 9 || !int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out nanos))
        {
            return false;
        }

        for (var i = digits.Length; i < 9; i++)
        {
            nanos *= 10;
        }

        return true;
    }

    private static JsonException Invalid()
    {
        return new JsonException("Value is not a protobuf duration of the form \"1.5s\" within ±10000 years.");
    }
}
