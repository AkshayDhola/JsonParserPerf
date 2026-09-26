using System.Reflection;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.Fields;

internal sealed class MapFieldBinding<TMessage, TKey, TKeyReader, TValue, TValueReader>(FieldDescriptor field, PropertyInfo property)
    : FieldBinding<TMessage>(field)
    where TMessage : class, IMessage<TMessage>, new()
    where TKey : notnull
    where TKeyReader : struct, IValueReader<TKey>
    where TValueReader : struct, IValueReader<TValue>
{
    private readonly Func<TMessage, MapField<TKey, TValue>> _get =
        property.GetMethod!.CreateDelegate<Func<TMessage, MapField<TKey, TValue>>>();

    private int _lastCount;

    public override void Read(ref Utf8JsonReader reader, TMessage message, ProtobufParserOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw JsonToken.ExpectedMap(reader.TokenType);
        }

        var entries = _get(message);
        if (_lastCount > 0 && entries.Count == 0)
        {
            MapFieldCapacity<TKey, TValue>.Dictionary?.Invoke(entries).EnsureCapacity(_lastCount);
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var count = Math.Min(entries.Count, MaxCapacityHint);
                if (_lastCount != count)
                {
                    _lastCount = count;
                }

                return;
            }

            var key = ValueReader.Read<TKeyReader, TKey>(ref reader, options);
            reader.Read();
            entries[key] = ValueReader.Read<TValueReader, TValue>(ref reader, options);
        }

        throw JsonToken.TruncatedMap();
    }
}
