using System.Buffers.Binary;
using System.Numerics;

namespace JsonParserPerf.Protobuf.Deserialization;

internal sealed class Utf8Cache<TValue> where TValue : class
{
    private const int Buckets = 512;

    private const int Ways = 2;

    private const int MaxLength = 128;

    private sealed class Entry(byte[] utf8, TValue value)
    {
        public readonly byte[] Utf8 = utf8;

        public readonly TValue Value = value;
    }

    private Entry?[]? _entries;

    private uint[]? _seen;

    public bool TryGet(ReadOnlySpan<byte> utf8, out TValue value, out ulong hash)
    {
        value = null!;
        hash = 0;

        if (utf8.Length > MaxLength)
        {
            return false;
        }

        hash = Hash(utf8);

        if (_entries is not { } entries)
        {
            return false;
        }

        // Both ways spelled out: no loop, and the bucket is range-checked once rather than per way. A read-only
        // span skips the covariance check a writable span of a reference type would make.
        var pair = new ReadOnlySpan<Entry?>(entries, Bucket(hash) * Ways, Ways);

        if (pair[0] is { } first && utf8.SequenceEqual(first.Utf8))
        {
            value = first.Value;
            return true;
        }

        if (pair[1] is { } second && utf8.SequenceEqual(second.Utf8))
        {
            value = second.Value;
            return true;
        }

        return false;
    }

    public void Add(ReadOnlySpan<byte> utf8, ulong hash, TValue value)
    {
        if (utf8.Length > MaxLength)
        {
            return;
        }

        var bucket = Bucket(hash);
        var fingerprint = (uint)(hash >> 32);
        var seen = _seen ??= new uint[Buckets];

        if (seen[bucket] != fingerprint)
        {
            seen[bucket] = fingerprint;
            return;
        }

        var entries = _entries ??= new Entry?[Buckets * Ways];
        var slot = bucket * Ways;
        entries[slot + 1] = entries[slot];
        entries[slot] = new Entry(utf8.ToArray(), value);
    }

    private static int Bucket(ulong hash)
    {
        return (int)(hash & (Buckets - 1));
    }

    private static ulong Hash(ReadOnlySpan<byte> utf8)
    {
        ulong first, last;

        if (utf8.Length >= 8)
        {
            first = BinaryPrimitives.ReadUInt64LittleEndian(utf8);
            last = BinaryPrimitives.ReadUInt64LittleEndian(utf8[^8..]);
        }
        else
        {
            first = 0;
            foreach (var b in utf8)
            {
                first = (first << 8) | b;
            }

            last = first;
        }

        // splitmix64's finalizer: cheap, and it spreads the low bits the mask reads.
        var hash = first ^ BitOperations.RotateLeft(last, 32) ^ (ulong)utf8.Length;
        hash ^= hash >> 30;
        hash *= 0xBF58476D1CE4E5B9UL;
        hash ^= hash >> 27;
        hash *= 0x94D049BB133111EBUL;
        return hash ^ (hash >> 31);
    }
}
