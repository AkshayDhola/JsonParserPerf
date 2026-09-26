using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;
using Type = System.Type;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal static class WellKnownReaderRegistry
{
    private static readonly Dictionary<Type, (Type? Reader, Delegate TopLevel)> Readers = Build();

    public static Type? ReaderTypeOf(Type type)
    {
        if (!Readers.TryGetValue(type, out var entry))
        {
            return null;
        }

        return entry.Reader ?? throw new NotSupportedException(
            $"A field of type '{type.FullName}' is generated as a nullable scalar, not as the wrapper message.");
    }

    public static ReadValueFunc<T>? ReaderOf<T>()
    {
        if (!Readers.TryGetValue(typeof(T), out var entry))
        {
            return null;
        }

        return (ReadValueFunc<T>)entry.TopLevel;
    }

    private static Dictionary<Type, (Type?, Delegate)> Build()
    {
        var readers = new Dictionary<Type, (Type?, Delegate)>();

        Add<Timestamp, TimestampReader>(readers);
        Add<Duration, DurationReader>(readers);
        Add<FieldMask, FieldMaskReader>(readers);
        Add<Any, AnyReader>(readers);
        Add<Struct, StructReader>(readers);
        Add<ListValue, ListValueReader>(readers);
        Add<Value, ProtobufValueReader>(readers);
        Add<Empty, EmptyReader>(readers);

        // Wrapper types: the JSON form is the bare wrapped value. Their fields are generated as nullable scalars,
        // so these are reached only at the top level and through Any.
        AddWrapper<DoubleValue, double, DoubleReader>(readers, v => new DoubleValue { Value = v });
        AddWrapper<FloatValue, float, FloatReader>(readers, v => new FloatValue { Value = v });
        AddWrapper<Int64Value, long, Int64Reader>(readers, v => new Int64Value { Value = v });
        AddWrapper<UInt64Value, ulong, UInt64Reader>(readers, v => new UInt64Value { Value = v });
        AddWrapper<Int32Value, int, Int32Reader>(readers, v => new Int32Value { Value = v });
        AddWrapper<UInt32Value, uint, UInt32Reader>(readers, v => new UInt32Value { Value = v });
        AddWrapper<BoolValue, bool, BoolReader>(readers, v => new BoolValue { Value = v });
        AddWrapper<StringValue, string, Utf8StringReader>(readers, v => new StringValue { Value = v });
        AddWrapper<BytesValue, ByteString, BytesReader>(readers, v => new BytesValue { Value = v });

        return readers;
    }

    private static void Add<T, TReader>(Dictionary<Type, (Type?, Delegate)> readers)
        where T : class, IMessage<T>, new()
        where TReader : struct, IValueReader<T>
    {
        ReadValueFunc<T> read = (ref Utf8JsonReader r, ProtobufParserOptions o) => ValueReader.Read<TReader, T>(ref r, o);
        readers.Add(typeof(T), (typeof(TReader), read));
    }

    private static void AddWrapper<T, TValue, TReader>(Dictionary<Type, (Type?, Delegate)> readers, Func<TValue, T> wrap)
        where T : class, IMessage<T>, new()
        where TReader : struct, IValueReader<TValue>
    {
        ReadValueFunc<T> read = (ref Utf8JsonReader r, ProtobufParserOptions o) => wrap(ValueReader.Read<TReader, TValue>(ref r, o));
        readers.Add(typeof(T), (null, read));
    }
}
