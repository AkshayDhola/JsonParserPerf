using System.Linq.Expressions;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Fields;
using JsonParserPerf.Protobuf.Deserialization.Values;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Protobuf.Deserialization;

internal sealed class ProtobufJsonParser<T> : IProtobufJsonParser
    where T : class, IMessage<T>, new()
{
    public static readonly ProtobufJsonParser<T> Instance = new();

    private readonly Func<T> _create;
    private readonly MessageDescriptor _descriptor;
    private readonly ReadValueFunc<T>? _wellKnownParser;

    private FieldBinding<T>[]? _fields;

    private ProtobufJsonParser()
    {
        _create = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        _descriptor = _create().Descriptor;
        _wellKnownParser = WellKnownReaderRegistry.ReaderOf<T>();
        ParseMessage = _wellKnownParser ?? ParseObject;
    }

    public bool IsWellKnownType => _wellKnownParser is not null;

    private ReadValueFunc<T> ParseMessage { get; }

    private FieldBinding<T>[] Fields => _fields ??= BindFields(_descriptor);

    IMessage IProtobufJsonParser.ParseMessage(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return ParseMessage(ref reader, options);
    }

    IMessage IProtobufJsonParser.ParseFields(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return ParseFields(ref reader, options);
    }

    internal T ParseObject(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected an object for '{_descriptor.FullName}' but found {reader.TokenType}.");
        }

        JsonToken.CheckDepth(reader, options);
        return ParseFields(ref reader, options);
    }

    private T ParseFields(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        var fields = Fields;
        var message = _create();
        var next = 0;
        var seenOneofs = new SeenOneofs();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return message;
            }

            var index = FindField(ref reader, fields, next);
            reader.Read();

            if (index < 0)
            {
                reader.Skip();
                continue;
            }

            next = index + 1;
            var field = fields[index];

            // JSON may name one member of a oneof, a null one included; Google rejects a second, so this does too.
            if (field.OneofIndex >= 0 && !seenOneofs.TryAdd(field.OneofIndex))
            {
                throw new JsonException(
                    $"Multiple values given for the oneof '{_descriptor.Oneofs[field.OneofIndex].Name}' of '{_descriptor.FullName}'.");
            }

            field.Read(ref reader, message, options);
        }

        throw new JsonException($"Unexpected end of JSON while reading '{_descriptor.FullName}'.");
    }

    private static int FindField(ref Utf8JsonReader reader, FieldBinding<T>[] fields, int expected)
    {
        var name = JsonToken.UnescapedUtf8(reader);
        if (expected < fields.Length && name.SequenceEqual(fields[expected].JsonName))
        {
            return expected;
        }

        return FindFieldByName(name, fields, expected);
    }

    private static int FindFieldByName(ReadOnlySpan<byte> name, FieldBinding<T>[] fields, int start)
    {
        for (var i = start; i < fields.Length; i++)
        {
            if (NameMatches(name, fields[i]))
            {
                return i;
            }
        }

        for (var i = 0; i < start && i < fields.Length; i++)
        {
            if (NameMatches(name, fields[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool NameMatches(ReadOnlySpan<byte> name, FieldBinding<T> field)
    {
        return name.Length > 0 && (NameEquals(name, field.JsonName) || NameEquals(name, field.ProtoName));
    }

    private static bool NameEquals(ReadOnlySpan<byte> name, byte[] candidate)
    {
        return candidate.Length == name.Length && candidate[0] == name[0] && name.SequenceEqual(candidate);
    }

    private static FieldBinding<T>[] BindFields(MessageDescriptor descriptor)
    {
        var fields = descriptor.Fields.InDeclarationOrder();
        var bindings = new FieldBinding<T>[fields.Count];

        for (var i = 0; i < bindings.Length; i++)
        {
            bindings[i] = FieldBinding<T>.Create(fields[i]);
        }

        return bindings;
    }
}