using System.Runtime.CompilerServices;
using System.Text.Json;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization.Values;

internal delegate TValue ReadValueFunc<TValue>(ref Utf8JsonReader reader, ProtobufParserOptions options);

internal static class ValueReader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue Read<TReader, TValue>(ref Utf8JsonReader reader, ProtobufParserOptions options)
        where TReader : struct, IValueReader<TValue>
    {
        if (TReader.Token != JsonTokenType.None && reader.TokenType != TReader.Token)
        {
            throw JsonToken.UnexpectedToken(TReader.Token, reader.TokenType, typeof(TValue).Name);
        }

        return TReader.Read(ref reader, options);
    }
}
