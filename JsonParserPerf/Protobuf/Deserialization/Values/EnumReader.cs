using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using NullValue = Google.Protobuf.WellKnownTypes.NullValue;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal struct EnumReader<TEnum> : IValueReader<TEnum>
    where TEnum : struct, Enum
{
    private static readonly byte[][] Names;

    private static readonly int[] Numbers;

    private static readonly bool NullIsValue = typeof(TEnum) == typeof(NullValue);

    static EnumReader()
    {
        var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
        Names = new byte[fields.Length][];
        Numbers = new int[fields.Length];

        for (var i = 0; i < fields.Length; i++)
        {
            Names[i] = Encoding.UTF8.GetBytes(fields[i].GetCustomAttribute<OriginalNameAttribute>()?.Name ?? fields[i].Name);
            Numbers[i] = (int)fields[i].GetRawConstantValue()!;
        }
    }

    public static TEnum Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        if (NullIsValue && reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        int number = reader.TokenType == JsonTokenType.String ?
            ReadName(ref reader) : Int32Reader.Read(ref reader, options);
        
        return Unsafe.As<int, TEnum>(ref number);
    }

    private static int ReadName(ref Utf8JsonReader reader)
    {
        var names = Names;
        var value = JsonToken.UnescapedUtf8(reader);

        for (var i = 0; i < names.Length; i++)
        {
            ReadOnlySpan<byte> name = names[i];
            if (name.Length == value.Length && name[0] == value[0] && value.SequenceEqual(name))
            {
                return Numbers[i];
            }
        }

        return 0;
    }
}
