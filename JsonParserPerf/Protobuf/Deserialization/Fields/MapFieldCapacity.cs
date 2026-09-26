using System.Linq.Expressions;
using System.Reflection;
using Google.Protobuf.Collections;

namespace JsonParserPerf.Protobuf.Deserialization.Fields;

internal static class MapFieldCapacity<TKey, TValue>
    where TKey : notnull
{
    public static readonly Func<MapField<TKey, TValue>, Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>>?
        Dictionary = Compile();

    private static Func<MapField<TKey, TValue>, Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>>? Compile()
    {
        var field = typeof(MapField<TKey, TValue>).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.FieldType != typeof(Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>))
        {
            return null;
        }

        var map = Expression.Parameter(typeof(MapField<TKey, TValue>));
        return Expression.Lambda<Func<MapField<TKey, TValue>, Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>>>(
            Expression.Field(map, field), map).Compile();
    }
}
