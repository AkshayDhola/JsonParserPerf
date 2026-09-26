using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct TimestampReader : IValueReader<Timestamp>
{
    private const long MinSeconds = -62_135_596_800L;

    private const long MaxSeconds = 253_402_300_799L;

    public static JsonTokenType Token => JsonTokenType.String;

    public static Timestamp Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (!TryParse(JsonToken.UnescapedUtf8(reader), out var seconds, out var nanos))
        {
            throw new JsonException("Value is not an RFC 3339 timestamp in the range 0001-01-01 to 9999-12-31.");
        }

        return new Timestamp { Seconds = seconds, Nanos = nanos };
    }

    private static bool TryParse(ReadOnlySpan<byte> utf8, out long seconds, out int nanos)
    {
        seconds = 0;
        nanos = 0;
        Span<char> dateTimeText = stackalloc char[19];

        if (utf8.Length < 20
            || Ascii.ToUtf16(utf8[..19], dateTimeText, out _) != OperationStatus.Done
            || !DateTime.TryParseExact(dateTimeText, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dateTime))
        {
            return false;
        }

        var rest = utf8[19..];
        if (rest[0] == (byte)'.')
        {
            var digits = rest[1..].IndexOfAnyExceptInRange((byte)'0', (byte)'9');
            if (digits < 0 || !DurationReader.TryParseNanos(rest.Slice(1, digits), out nanos))
            {
                return false;
            }

            rest = rest[(digits + 1)..];
        }

        if (!TryParseOffset(rest, out var offset))
        {
            return false;
        }

        seconds = ((dateTime.Ticks - DateTime.UnixEpoch.Ticks) / TimeSpan.TicksPerSecond) - offset;
        return seconds is >= MinSeconds and <= MaxSeconds;
    }

    private static bool TryParseOffset(ReadOnlySpan<byte> utf8, out long offset)
    {
        offset = 0;
        if (utf8.SequenceEqual("Z"u8))
        {
            return true;
        }

        if (utf8.Length != 6 || utf8[0] is not ((byte)'+' or (byte)'-') || utf8[3] != (byte)':'
            || !int.TryParse(utf8.Slice(1, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var hours) || hours > 23
            || !int.TryParse(utf8.Slice(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var minutes) || minutes > 59)
        {
            return false;
        }

        offset = (hours * 3600L) + (minutes * 60L);
        if (utf8[0] == (byte)'-')
        {
            offset = -offset;
        }

        return true;
    }
}