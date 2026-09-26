using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.Fields;

internal abstract class FieldBinding<TMessage>
    where TMessage : class, IMessage<TMessage>, new()
{
    protected FieldBinding(FieldDescriptor field)
    {
        JsonName = Encoding.UTF8.GetBytes(field.JsonName);
        ProtoName = Encoding.UTF8.GetBytes(field.Name);
        OneofIndex = field.ContainingOneof?.Index ?? -1;
    }

    public byte[] JsonName { get; }

    public byte[] ProtoName { get; }

    public int OneofIndex { get; }

    protected const int MaxCapacityHint = 1024;

    public abstract void Read(ref Utf8JsonReader reader, TMessage message, ProtobufParserOptions options);

    public static FieldBinding<TMessage> Create(FieldDescriptor field)
    {
        var property = typeof(TMessage).GetProperty(field.PropertyName)
                       ?? throw new InvalidOperationException(
                           $"Type '{typeof(TMessage).FullName}' has no property '{field.PropertyName}' for field '{field.FullName}'.");
        var type = property.PropertyType;

        Type binding;
        if (field.IsMap)
        {
            var args = type.GetGenericArguments();
            var valueField = field.MessageType.Fields.InFieldNumberOrder()[1];
            binding = typeof(MapFieldBinding<,,,,>).MakeGenericType(
                typeof(TMessage), args[0], ValueReaderRegistry.MapKeyReaderTypeOf(args[0]), args[1], ValueReaderRegistry.ReaderTypeOf(args[1], valueField));
        }
        else if (field.IsRepeated)
        {
            var item = type.GetGenericArguments()[0];
            binding = typeof(RepeatedFieldBinding<,,>).MakeGenericType(typeof(TMessage), item, ValueReaderRegistry.ReaderTypeOf(item, field));
        }
        else
        {
            binding = typeof(SingularFieldBinding<,,>).MakeGenericType(typeof(TMessage), type, ValueReaderRegistry.ReaderTypeOf(type, field));
        }

        return (FieldBinding<TMessage>)Activator.CreateInstance(binding, field, property)!;
    }
}
