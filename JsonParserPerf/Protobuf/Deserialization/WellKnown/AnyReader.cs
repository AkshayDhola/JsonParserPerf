using System.Buffers;
using System.Collections.Concurrent;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct AnyReader : IValueReader<Any>
{
    private const string TypeMember = "@type";
    private const string ValueMember = "value";

    private static readonly ConcurrentDictionary<MessageDescriptor, IProtobufJsonParser> ParsersByDescriptor = new();

    public static JsonTokenType Token => JsonTokenType.StartObject;

    public static Any Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        // Canonical output writes @type first, which lets the payload stream straight into its message reader.
        if (TryReadTypeFirst(ref reader, options, out var any))
        {
            return any;
        }

        return ReadBuffered(ref reader, options);
    }

    private static bool TryReadTypeFirst(ref Utf8JsonReader reader, ProtobufParserOptions options, out Any any)
    {
        any = null!;

        // Probe on a copy so a payload with @type elsewhere leaves the real reader untouched.
        var probe = reader;

        if (!probe.Read() || probe.TokenType != JsonTokenType.PropertyName || !probe.ValueTextEquals(TypeMember)
            || !probe.Read() || probe.TokenType != JsonTokenType.String)
        {
            return false;
        }

        // Through the interning cache, so the URL a payload repeats is not materialized again.
        var typeUrl = Utf8StringReader.ReadText(ref probe, options);
        var payloadParser = ResolvePayloadParser(typeUrl, options);
        IMessage payload;

        if (payloadParser.IsWellKnownType)
        {
            if (!probe.Read() || probe.TokenType != JsonTokenType.PropertyName || !probe.ValueTextEquals(ValueMember)
                || !probe.Read())
            {
                return false;
            }

            payload = payloadParser.ParseMessage(ref probe, options);

            if (!probe.Read() || probe.TokenType != JsonTokenType.EndObject)
            {
                return false;
            }
        }
        else
        {
            payload = payloadParser.ParseFields(ref probe, options);
        }

        reader = probe;
        any = new Any { TypeUrl = typeUrl, Value = payload.ToByteString() };
        return true;
    }

    private static Any ReadBuffered(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty(TypeMember, out var typeMember) || typeMember.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Any is missing its @type member.");
        }

        var typeUrl = typeMember.GetString()!;
        var payloadParser = ResolvePayloadParser(typeUrl, options);

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            if (payloadParser.IsWellKnownType)
            {
                if (!root.TryGetProperty(ValueMember, out var valueMember))
                {
                    throw new JsonException($"Any of type '{typeUrl}' is missing its value member.");
                }

                valueMember.WriteTo(writer);
            }
            else
            {
                writer.WriteStartObject();
                foreach (var property in root.EnumerateObject())
                {
                    if (!property.NameEquals(TypeMember))
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }
        }

        var payloadJson = new Utf8JsonReader(buffer.WrittenSpan);
        payloadJson.Read();
        var payload = payloadParser.ParseMessage(ref payloadJson, options);

        return new Any { TypeUrl = typeUrl, Value = payload.ToByteString() };
    }

    private static IProtobufJsonParser ResolvePayloadParser(string typeUrl, ProtobufParserOptions options)
    {
        var typeName = typeUrl[(typeUrl.LastIndexOf('/') + 1)..];
        var descriptor = options.TypeRegistry.Find(typeName)
                         ?? throw new JsonException($"Type '{typeUrl}' is not in the parser's type registry.");
        return ParsersByDescriptor.GetOrAdd(descriptor, static d =>
            (IProtobufJsonParser)typeof(ProtobufJsonParser<>).MakeGenericType(d.ClrType)
                .GetField(nameof(ProtobufJsonParser<Empty>.Instance))!.GetValue(null)!);
    }
}
