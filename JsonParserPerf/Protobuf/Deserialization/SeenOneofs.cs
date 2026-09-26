namespace JsonParserPerf.Protobuf.Deserialization;

internal struct SeenOneofs
{
    private ulong _bits;
    private HashSet<int>? _overflow;

    public bool TryAdd(int index)
    {
        if (index >= 64)
        {
            return (_overflow ??= new HashSet<int>()).Add(index);
        }

        var bit = 1UL << index;
        var added = (_bits & bit) == 0;
        _bits |= bit;
        return added;
    }
}