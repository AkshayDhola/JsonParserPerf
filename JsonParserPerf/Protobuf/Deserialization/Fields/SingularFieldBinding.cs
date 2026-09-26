using System.Reflection;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace JsonParserPerf.Protobuf.Deserialization.Fields;

internal sealed class SingularFieldBinding<TMessage, TValue, TReader>(FieldDescriptor field, PropertyInfo property)
    : FieldBinding<TMessage>(field)
    where TMessage : class, IMessage<TMessage>, new()
    where TReader : struct, IValueReader<TValue>
{
    private readonly Action<TMessage, TValue> _set = property.SetMethod!.CreateDelegate<Action<TMessage, TValue>>();

    public override void Read(ref Utf8JsonReader reader, TMessage message, ProtobufParserOptions options)
    {
        // JSON null leaves a field at its default, except google.protobuf.Value, where null is the NullValue.
        if (reader.TokenType != JsonTokenType.Null || typeof(TValue) == typeof(Value))
        {
            _set(message, ValueReader.Read<TReader, TValue>(ref reader, options));
        }
    }
}
