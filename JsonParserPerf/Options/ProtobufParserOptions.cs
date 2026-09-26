using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Protobuf;
using JsonParserPerf.Protobuf.Deserialization;

namespace JsonParserPerf.Options;

public sealed class ProtobufParserOptions
{
    public const int DefaultRecursionLimit = 100;
    public static ProtobufParserOptions Default { get; } = new();
    
    public int RecursionLimit { get; }
    public bool InternValues { get; }
    
    private readonly TypeRegistry? _typeRegistry;
    public TypeRegistry TypeRegistry => _typeRegistry ?? ProtobufTypeRegistry.FromLoadedAssemblies();

    internal Utf8Cache<string>? StringCache { get; }
    internal Utf8Cache<ByteString>? BytesCache { get; }
    
    public ProtobufParserOptions(
        TypeRegistry typeRegistry,
        int recursionLimit = DefaultRecursionLimit,
        bool internValues = false)
    {
        ArgumentNullException.ThrowIfNull(typeRegistry);
        ArgumentOutOfRangeException.ThrowIfLessThan(recursionLimit, 1);

        _typeRegistry = typeRegistry;
        RecursionLimit = recursionLimit;
        InternValues = internValues;
        
        if (internValues)
        {
            StringCache = new Utf8Cache<string>();
            BytesCache = new Utf8Cache<ByteString>();
        }
    }
    
    private ProtobufParserOptions()
    {
        RecursionLimit = DefaultRecursionLimit;
        InternValues = false;
    }
}