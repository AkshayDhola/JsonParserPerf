using System.Text.Json;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Json;

namespace JsonParserPerf.Protobuf;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddProtobuf(
        this JsonSerializerOptions options,
        TypeRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Converters.Add(registry is null
            ? new ProtobufJsonConverterFactory()
            : new ProtobufJsonConverterFactory(new ProtobufParserOptions(registry)));
        return options;
    }
}