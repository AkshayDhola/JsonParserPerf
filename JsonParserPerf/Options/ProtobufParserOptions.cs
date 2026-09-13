using Google.Protobuf.Reflection;
using JsonParserPerf.Protobuf;

namespace JsonParserPerf.Options;

public sealed class ProtobufParserOptions
{
    public const int DefaultRecursionLimit = 100;
    public static ProtobufParserOptions Default { get; } = new();
    
    public int RecursionLimit { get; }
    public bool InternStrings { get; }
    
    private readonly TypeRegistry? _typeRegistry;
    public TypeRegistry TypeRegistry => _typeRegistry ?? ProtobufTypeRegistry.GetTypeRegistry();

    
    public ProtobufParserOptions(
        TypeRegistry typeRegistry,
        int recursionLimit = DefaultRecursionLimit,
        bool internStrings = false)
    {
        ArgumentNullException.ThrowIfNull(typeRegistry);
        ArgumentOutOfRangeException.ThrowIfLessThan(recursionLimit, 1);

        _typeRegistry = typeRegistry;
        RecursionLimit = recursionLimit;
        InternStrings = internStrings;
    }
    
    private ProtobufParserOptions()
    {
        RecursionLimit = DefaultRecursionLimit;
        InternStrings = false;
    }
}