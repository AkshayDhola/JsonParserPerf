using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using JsonParserPerf.Options;

namespace JsonParserPerf.Protobuf.Json;

public sealed class ProtobufJsonConverterFactory(ProtobufParserOptions options) : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, JsonConverter> _converters = new();
    private readonly ProtobufParserOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public ProtobufJsonConverterFactory() : this(ProtobufParserOptions.Default)
    {
    }
    
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IMessage).IsAssignableFrom(typeToConvert)
               && !typeToConvert.IsAbstract
               && typeToConvert.GetConstructor(Type.EmptyTypes) is not null
               && typeof(IMessage<>).MakeGenericType(typeToConvert).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return _converters.GetOrAdd(typeToConvert, type => (JsonConverter)Activator.CreateInstance(
            typeof(ProtobufJsonConverter<>).MakeGenericType(type),
            _options)!);
    }
}