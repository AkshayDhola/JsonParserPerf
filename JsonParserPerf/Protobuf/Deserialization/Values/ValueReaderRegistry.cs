using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal static class ValueReaderRegistry
{
    private static readonly Dictionary<Type, Type> Readers = Build();

    private static readonly Dictionary<Type, Type> MapKeyReaders = BuildMapKeys();

    public static Type ReaderTypeOf(Type type, FieldDescriptor field)
    {
        if (Readers.TryGetValue(type, out var reader))
        {
            return reader;
        }

        if (Nullable.GetUnderlyingType(type) is { } wrapped)
        {
            return typeof(NullableReader<,>).MakeGenericType(wrapped, ReaderTypeOf(wrapped, field));
        }

        if (type.IsEnum)
        {
            return typeof(EnumReader<>).MakeGenericType(type);
        }

        if (!typeof(IMessage).IsAssignableFrom(type))
        {
            throw new NotSupportedException($"Field '{field.FullName}' has unsupported CLR type '{type}'.");
        }

        return WellKnownReaderRegistry.ReaderTypeOf(type)
               ?? typeof(MessageValueReader<>).MakeGenericType(type);
    }

    public static Type MapKeyReaderTypeOf(Type type)
    {
        if (!MapKeyReaders.TryGetValue(type, out var reader))
        {
            throw new NotSupportedException($"Map key type '{type}' is not supported.");
        }

        return reader;
    }

    private static Dictionary<Type, Type> Build()
    {
        var readers = new Dictionary<Type, Type>();

        Add<int, Int32Reader>(readers);
        Add<long, Int64Reader>(readers);
        Add<uint, UInt32Reader>(readers);
        Add<ulong, UInt64Reader>(readers);
        Add<double, DoubleReader>(readers);
        Add<float, FloatReader>(readers);
        Add<bool, BoolReader>(readers);
        Add<string, Utf8StringReader>(readers);
        Add<ByteString, BytesReader>(readers);

        return readers;
    }

    private static Dictionary<Type, Type> BuildMapKeys()
    {
        var readers = new Dictionary<Type, Type>();

        Add<string, StringKeyReader>(readers);
        Add<bool, BoolKeyReader>(readers);
        Add<int, Int32Reader>(readers);
        Add<long, Int64Reader>(readers);
        Add<uint, UInt32Reader>(readers);
        Add<ulong, UInt64Reader>(readers);

        return readers;
    }

    private static void Add<T, TReader>(Dictionary<Type, Type> readers)
        where TReader : struct, IValueReader<T>
    {
        readers.Add(typeof(T), typeof(TReader));
    }
}
