using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Benchmarks;

public static class BenchmarkData
{
    private static readonly string[] FirstNames =
        ["Aarav", "Sofia", "Liam", "Mei", "Noah", "Amara", "Lucas", "Priya", "Mateo", "Yuki", "Elena", "Omar"];

    private static readonly string[] LastNames =
        ["Sharma", "García", "Smith", "Chen", "Müller", "Okafor", "Rossi", "Kim", "Silva", "Novak", "Haddad"];

    private static readonly string[] Countries = ["IN", "US", "DE", "BR", "JP", "NG", "IT", "KR", "FR", "GB"];

    private static readonly string[] Tags =
        ["premium", "newsletter", "beta", "churn-risk", "enterprise", "mobile", "referral", "vip"];

    public static TypeRegistry Registry { get; } = TypeRegistry.FromFiles(
        EnumTypesReflection.Descriptor,
        LargePayloadReflection.Descriptor,
        MapTypesReflection.Descriptor,
        MessageTypesReflection.Descriptor,
        OneofTypesReflection.Descriptor,
        ScalarTypesReflection.Descriptor,
        SimplePayloadReflection.Descriptor,
        WellKnownTypesReflection.Descriptor);

    public static JsonFormatter Formatter { get; } = new(JsonFormatter.Settings.Default.WithTypeRegistry(Registry));

    public static JsonParser GoogleParser { get; } =
        new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true).WithTypeRegistry(Registry));

    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        Converters = { new ProtobufJsonConverterFactory(new ProtobufParserOptions(Registry)) },
    };

    public static CustomerList Customers(int count)
    {
        var random = new Random(42);
        var list = new CustomerList();

        for (var i = 0; i < count; i++)
        {
            var first = Pick(random, FirstNames);
            var last = Pick(random, LastNames);
            var customer = new Customer
            {
                Id = 100_000 + i,
                Name = $"{first} {last}",
                Email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}{i}@example.com",
                Country = Pick(random, Countries),
                Age = random.Next(18, 81),
                Balance = Math.Round(random.NextDouble() * 25_000, 2),
                Active = random.NextDouble() < 0.8,
            };

            var tagCount = random.Next(0, 5);
            for (var t = 0; t < tagCount; t++)
            {
                customer.Tags.Add(Pick(random, Tags));
            }

            list.Customers.Add(customer);
        }

        return list;
    }

    public static LargePayload Large(int items)
    {
        var random = new Random(7);
        var payload = new LargePayload
        {
            PayloadNameField = "nightly-export",
            RecursiveMessageField = Recursive(random, depth: 6),
        };

        for (var i = 0; i < items; i++)
        {
            payload.ScalarTypesFields.Add(Scalars(random, i));
            payload.RepeatedScalarTypesFields.Add(RepeatedScalars(random));
            payload.EnumTypesFields.Add(Enums(random));
            payload.WellKnownTypesFields.Add(WellKnown(random, i));
            payload.MapValueTypesFields.Add(MapValues(random, i));
            payload.MessageTypesMap[$"entry-{i}"] = Messages(random, i);
        }

        return payload;
    }

    private static ScalarTypes Scalars(Random random, int i)
    {
        return new ScalarTypes
        {
            DoubleField = Math.Round(random.NextDouble() * 1_000, 4),
            FloatField = (float)Math.Round(random.NextDouble() * 100, 2),
            Int32Field = random.Next(-1_000_000, 1_000_000),
            Int64Field = random.NextInt64(-10_000_000_000, 10_000_000_000),
            Uint32Field = (uint)random.Next(0, int.MaxValue),
            Uint64Field = (ulong)random.NextInt64(0, long.MaxValue),
            Sint32Field = random.Next(-500, 500),
            Sint64Field = random.NextInt64(-500_000, 500_000),
            Fixed32Field = (uint)random.Next(),
            Fixed64Field = (ulong)random.NextInt64(),
            Sfixed32Field = random.Next(-100, 100),
            Sfixed64Field = random.NextInt64(-100_000, 100_000),
            BoolField = random.Next(2) == 0,
            StringField = $"record {i} — {Pick(random, FirstNames)}",
            BytesField = ByteString.CopyFrom(RandomBytes(random, 24)),
        };
    }

    private static RepeatedScalarTypes RepeatedScalars(Random random)
    {
        var message = new RepeatedScalarTypes();
        for (var i = 0; i < 5; i++)
        {
            message.DoubleFields.Add(Math.Round(random.NextDouble() * 100, 3));
            message.Int32Fields.Add(random.Next(-1000, 1000));
            message.Int64Fields.Add(random.NextInt64(0, 1_000_000_000_000));
            message.BoolFields.Add(random.Next(2) == 0);
            message.StringFields.Add(Pick(random, Tags));
        }

        message.BytesFields.Add(ByteString.CopyFrom(RandomBytes(random, 8)));
        return message;
    }

    private static EnumTypes Enums(Random random)
    {
        var message = new EnumTypes
        {
            TopLevelEnumField = (TopLevelEnum)random.Next(0, 3),
            NestedEnumField = (EnumTypes.Types.NestedEnum)random.Next(0, 3),
        };
        message.TopLevelEnumFields.Add([TopLevelEnum.First, TopLevelEnum.Second, TopLevelEnum.Negative]);
        message.TopLevelEnumMap["primary"] = TopLevelEnum.First;
        message.TopLevelEnumMap["fallback"] = TopLevelEnum.Second;
        return message;
    }

    private static WellKnownTypes WellKnown(Random random, int i)
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(random.Next(0, 20_000_000));

        return new WellKnownTypes
        {
            DoubleValueField = Math.Round(random.NextDouble() * 50, 2),
            FloatValueField = 1.5f,
            Int64ValueField = random.NextInt64(0, 1_000_000_000),
            Uint64ValueField = (ulong)random.NextInt64(0, 1_000_000),
            Int32ValueField = random.Next(0, 1000),
            Uint32ValueField = (uint)random.Next(0, 1000),
            BoolValueField = true,
            StringValueField = Pick(random, Countries),
            BytesValueField = ByteString.CopyFrom(RandomBytes(random, 12)),
            TimestampField = Timestamp.FromDateTime(createdAt.AddTicks(random.Next(0, 10_000_000))),
            DurationField = Duration.FromTimeSpan(TimeSpan.FromMilliseconds(random.Next(1, 90_000))),
            FieldMaskField = new FieldMask { Paths = { "string_value_field", "timestamp_field" } },
            StructField = new Struct
            {
                Fields =
                {
                    ["source"] = Value.ForString("import"),
                    ["attempt"] = Value.ForNumber(random.Next(1, 4)),
                    ["retry"] = Value.ForBool(false),
                    ["labels"] = Value.ForList(Value.ForString("eu"), Value.ForString("batch")),
                },
            },
            ValueField = Value.ForNumber(i),
            ListValueField = new ListValue { Values = { Value.ForNumber(1), Value.ForString("two"), Value.ForNull() } },
            AnyField = Any.Pack(new LeafMessage { LeafStringField = $"leaf-{i}", LeafInt32Field = i }),
            EmptyField = new Empty(),
            NullValueField = NullValue.NullValue,
        };
    }

    private static MapValueTypes MapValues(Random random, int i)
    {
        var message = new MapValueTypes();
        for (var k = 0; k < 3; k++)
        {
            var key = $"key-{k}";
            message.DoubleValueMap[key] = Math.Round(random.NextDouble(), 4);
            message.Int32ValueMap[key] = random.Next();
            message.Int64ValueMap[key] = random.NextInt64();
            message.BoolValueMap[key] = k % 2 == 0;
            message.StringValueMap[key] = Pick(random, LastNames);
            message.BytesValueMap[key] = ByteString.CopyFrom(RandomBytes(random, 6));
            message.EnumValueMap[key] = TopLevelEnum.Second;
            message.MessageValueMap[key] = new LeafMessage { LeafStringField = $"m{i}-{k}", LeafInt32Field = k };
        }

        return message;
    }

    private static MessageTypes Messages(Random random, int i)
    {
        var message = new MessageTypes
        {
            LeafMessageField = new LeafMessage { LeafStringField = Pick(random, FirstNames), LeafInt32Field = i },
            NestedMessageField = new MessageTypes.Types.NestedMessage
            {
                NestedStringField = Pick(random, Countries),
                NestedLeafMessageField = new LeafMessage { LeafInt32Field = random.Next(100) },
            },
        };
        message.LeafMessageFields.Add(new LeafMessage { LeafStringField = "a" });
        message.LeafMessageFields.Add(new LeafMessage { LeafStringField = "b", LeafInt32Field = 2 });
        message.LeafMessageMap["first"] = new LeafMessage { LeafInt32Field = 1 };
        return message;
    }

    private static RecursiveMessage Recursive(Random random, int depth)
    {
        var node = new RecursiveMessage { NameField = $"level-{depth}" };
        if (depth > 0)
        {
            node.ChildField = Recursive(random, depth - 1);
            node.ChildFields.Add(new RecursiveMessage { NameField = $"leaf-{depth}" });
        }

        return node;
    }

    private static string Pick(Random random, string[] values)
    {
        return values[random.Next(values.Length)];
    }

    private static byte[] RandomBytes(Random random, int length)
    {
        var bytes = new byte[length];
        random.NextBytes(bytes);
        return bytes;
    }
}
