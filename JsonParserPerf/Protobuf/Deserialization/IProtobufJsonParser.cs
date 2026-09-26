using System.Text.Json;
using Google.Protobuf;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Deserialization;

internal interface IProtobufJsonParser
{
    bool IsWellKnownType { get; }
    IMessage ParseMessage(ref Utf8JsonReader reader, ProtobufParserOptions options);
    IMessage ParseFields(ref Utf8JsonReader reader, ProtobufParserOptions options);
}