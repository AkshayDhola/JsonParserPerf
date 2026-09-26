using System.Text.Json;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal struct NullableReader<T, TReader> : IValueReader<T?>
    where T : struct
    where TReader : struct, IValueReader<T>
{
    public static T? Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        return ValueReader.Read<TReader, T>(ref reader, options);
    }
}
