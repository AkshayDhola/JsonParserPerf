using System.Reflection;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.Fields;

internal sealed class RepeatedFieldBinding<TMessage, TItem, TReader>(FieldDescriptor field, PropertyInfo property)
    : FieldBinding<TMessage>(field)
    where TMessage : class, IMessage<TMessage>, new()
    where TReader : struct, IValueReader<TItem>
{
    private readonly Func<TMessage, RepeatedField<TItem>> _get =
        property.GetMethod!.CreateDelegate<Func<TMessage, RepeatedField<TItem>>>();

    private int _lastCount;

    public override void Read(ref Utf8JsonReader reader, TMessage message, ProtobufParserOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw JsonToken.ExpectedArray(reader.TokenType);
        }

        var items = _get(message);
        if (_lastCount > 0 && items.Count == 0)
        {
            items.Capacity = _lastCount;
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                var count = Math.Min(items.Count, MaxCapacityHint);
                if (_lastCount != count)
                {
                    _lastCount = count;
                }

                return;
            }

            items.Add(ValueReader.Read<TReader, TItem>(ref reader, options));
        }

        throw JsonToken.TruncatedRepeated();
    }
}
