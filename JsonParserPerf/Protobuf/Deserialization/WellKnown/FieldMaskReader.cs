using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Protobuf.Deserialization.WellKnown;

internal struct FieldMaskReader : IValueReader<FieldMask>
{
    public static JsonTokenType Token => JsonTokenType.String;

    public static FieldMask Read(ref Utf8JsonReader reader, ProtobufParserOptions options)
    {
        var mask = new FieldMask();
        foreach (var path in reader.GetString()!.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            mask.Paths.Add(ToSnakeCase(path));
        }

        return mask;
    }

    private static string ToSnakeCase(string camel)
    {
        var upper = 0;
        foreach (var c in camel)
        {
            if (c == '_')
            {
                throw new JsonException("Field mask paths must be camelCase, not snake_case.");
            }

            if (char.IsAsciiLetterUpper(c))
            {
                upper++;
            }
        }

        if (upper == 0)
        {
            return camel;
        }

        return string.Create(camel.Length + upper, camel, static (snake, camel) =>
        {
            var i = 0;
            foreach (var c in camel)
            {
                if (char.IsAsciiLetterUpper(c))
                {
                    snake[i++] = '_';
                    snake[i++] = char.ToLowerInvariant(c);
                }
                else
                {
                    snake[i++] = c;
                }
            }
        });
    }
}
