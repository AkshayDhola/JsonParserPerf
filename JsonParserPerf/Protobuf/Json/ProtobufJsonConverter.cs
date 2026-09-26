using System.Text.Json.Serialization;
using System.Text.Json;
using Google.Protobuf;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization;

namespace JsonParserPerf.Protobuf.Json;

public sealed class ProtobufJsonConverter<T>(ProtobufParserOptions options) : JsonConverter<T>
    where T : class, IMessage<T>, new()
{
    private readonly ProtobufParserOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public ProtobufJsonConverter() : this(ProtobufParserOptions.Default)
    {
    }

    public override bool HandleNull => true;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // JSON null is no message, except google.protobuf.Value, where null is the NullValue.
        if (reader.TokenType == JsonTokenType.Null && typeof(T) != typeof(Google.Protobuf.WellKnownTypes.Value))
        {
            return null;
        }

        return (T)((IProtobufJsonParser)ProtobufJsonParser<T>.Instance).ParseMessage(ref reader, _options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotSupportedException(
            $"Serializing protobuf message '{typeof(T).FullName}' through System.Text.Json is not supported; use JsonFormatter.");
    }
}