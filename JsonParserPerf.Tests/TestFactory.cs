using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using JsonParserPerf.Options;
using JsonParserPerf.Protobuf.Deserialization;
using JsonParserPerf.Protobuf.Deserialization.Values;
using JsonParserPerf.Protobuf.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests;

public static class TestFactory
{
    public const string TypeUrlPrefix = "type.googleapis.com/jsonparserperf.protobufdatamodels.";

    public static TypeRegistry Registry { get; } = TypeRegistry.FromFiles(
        EnumTypesReflection.Descriptor,
        LargePayloadReflection.Descriptor,
        MapTypesReflection.Descriptor,
        MessageTypesReflection.Descriptor,
        OneofTypesReflection.Descriptor,
        ScalarTypesReflection.Descriptor,
        SimplePayloadReflection.Descriptor,
        WellKnownTypesReflection.Descriptor);

    public static JsonParser GoogleParser { get; } =
        new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true).WithTypeRegistry(Registry));

    private static readonly JsonSerializerOptions DefaultSerializerOptions = SerializerOptions();

    public static ProtobufParserOptions ParserOptions(
        int recursionLimit = ProtobufParserOptions.DefaultRecursionLimit,
        bool internValues = false)
    {
        return new ProtobufParserOptions(Registry, recursionLimit, internValues);
    }

    public static JsonSerializerOptions SerializerOptions(ProtobufParserOptions? parserOptions = null)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ProtobufJsonConverterFactory(parserOptions ?? ParserOptions()));
        return options;
    }

    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultSerializerOptions);
    }

    public static T? DeserializeUtf8<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetBytes(json), options ?? DefaultSerializerOptions);
    }

    public static string Quote(string value)
    {
        return JsonSerializer.Serialize(value);
    }

    public static string NestedRecursiveJson(int depth)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < depth; i++)
        {
            builder.Append("""{"nameField":"n","childField":""");
        }

        builder.Append("null");
        return builder.Append('}', depth).ToString();
    }

    public static string NestedArrayJson(int depth)
    {
        return new string('[', depth) + new string(']', depth);
    }

    internal static TValue ReadValue<TReader, TValue>(string json, ProtobufParserOptions? options = null)
        where TReader : struct, IValueReader<TValue>
    {
        return ReadValue<TReader, TValue>(Encoding.UTF8.GetBytes(json), options);
    }

    internal static TValue ReadValue<TReader, TValue>(byte[] utf8, ProtobufParserOptions? options = null)
        where TReader : struct, IValueReader<TValue>
    {
        var reader = new Utf8JsonReader(utf8);
        reader.Read();
        return ValueReader.Read<TReader, TValue>(ref reader, options ?? ParserOptions());
    }

    internal static TValue ReadKey<TReader, TValue>(string key, ProtobufParserOptions? options = null)
        where TReader : struct, IValueReader<TValue>
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes($$"""{{{Quote(key)}}:0}"""));
        reader.Read();
        reader.Read();
        return ValueReader.Read<TReader, TValue>(ref reader, options ?? ParserOptions());
    }

    internal static T ParseObject<T>(string json, ProtobufParserOptions? options = null)
        where T : class, IMessage<T>, new()
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return ProtobufJsonParser<T>.Instance.ParseObject(ref reader, options ?? ParserOptions());
    }

    internal static bool TryReadNumber<T>(string json, out T value)
        where T : struct, System.Numerics.INumberBase<T>
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return JsonToken.TryReadNumber(reader, out value);
    }

    internal static bool TryReadDouble(string json, out double value)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return JsonToken.TryReadDouble(reader, out value);
    }

    internal static byte[] UnescapedUtf8(string json)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return JsonToken.UnescapedUtf8(reader).ToArray();
    }

    internal static void CheckDepthAt(int depth, ProtobufParserOptions options)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(NestedArrayJson(depth + 1)));
        for (var i = 0; i <= depth; i++)
        {
            reader.Read();
        }

        JsonToken.CheckDepth(reader, options);
    }

    public static TheoryData<string> ScalarPayloads() => new()
    {
        "{}",
        """{"doubleField":1.5,"floatField":-2.25}""",
        """{"doubleField":"1.5","floatField":"-2.25"}""",
        """{"doubleField":"NaN"}""",
        """{"doubleField":"Infinity","floatField":"-Infinity"}""",
        """{"doubleField":1e300,"floatField":1e38}""",
        """{"int32Field":-2147483648,"int64Field":"-9223372036854775808"}""",
        """{"int32Field":2147483647,"int64Field":"9223372036854775807"}""",
        """{"uint32Field":4294967295,"uint64Field":"18446744073709551615"}""",
        """{"sint32Field":-2147483648,"sint64Field":"-9223372036854775808"}""",
        """{"fixed32Field":4294967295,"fixed64Field":"18446744073709551615"}""",
        """{"sfixed32Field":-2147483648,"sfixed64Field":"-9223372036854775808"}""",
        """{"int64Field":123,"uint64Field":456,"sint64Field":-789,"sfixed64Field":-1}""",
        """{"boolField":true}""",
        """{"boolField":false}""",
        """{"stringField":"text with \"escapes\" and é"}""",
        """{"stringField":""}""",
        """{"bytesField":"aGVsbG8="}""",
        """{"bytesField":""}""",
        """{"bytesField":"\/w=="}""",
        """{"int32Field":1.0,"int64Field":"2"}""",
        """{"int32Field":1e3,"int64Field":"1e3"}""",
        """{"doubleField":null,"int32Field":null,"stringField":null,"bytesField":null,"boolField":null}""",
        """{"double_field":1.5,"sfixed64_field":"-1","bytes_field":"aGk="}""",
        """{"doubleField":1.5,"unknownField":{"nested":[1,2]}}""",
    };

    public static TheoryData<string> InvalidScalarPayloads() => new()
    {
        """{"int32Field":2147483648}""",
        """{"int32Field":-2147483649}""",
        """{"uint32Field":4294967296}""",
        """{"uint32Field":-1}""",
        """{"uint64Field":-1}""",
        """{"int32Field":1.5}""",
        """{"int64Field":"nine"}""",
        """{"boolField":"true"}""",
        """{"stringField":5}""",
        """{"bytesField":"not base64"}""",
        """{"bytesField":"aGk"}""",
        """{"doubleField":"1.5x"}""",
        """{"doubleField":"nan"}""",
        """{"doubleField":"1e400"}""",
        """{"floatField":1e39}""",
        """{"doubleField":{}}""",
        """{"doubleField":[]}""",
        "[]",
        "5",
        "\"text\"",
        "true",
    };

    public static TheoryData<string> RepeatedScalarPayloads() => new()
    {
        "{}",
        """{"doubleFields":[1.5,-2.5,0],"floatFields":[1.25]}""",
        """{"doubleFields":["NaN","Infinity","-Infinity"]}""",
        """{"int32Fields":[-2147483648,0,2147483647]}""",
        """{"int64Fields":["-9223372036854775808","0","9223372036854775807"]}""",
        """{"uint32Fields":[0,4294967295],"uint64Fields":["0","18446744073709551615"]}""",
        """{"sint32Fields":[-1,1],"sint64Fields":["-1","1"]}""",
        """{"fixed32Fields":[1,2],"fixed64Fields":["1","2"]}""",
        """{"sfixed32Fields":[-1,2],"sfixed64Fields":["-1","2"]}""",
        """{"boolFields":[true,false,true]}""",
        """{"stringFields":["a","","é"]}""",
        """{"bytesFields":["aGk=","",  "aGVsbG8="]}""",
        """{"doubleFields":[],"stringFields":[],"bytesFields":[]}""",
        """{"doubleFields":null,"stringFields":null}""",
        """{"int32Fields":[1,"2",3.0]}""",
        """{"int32_fields":[1,2],"bytes_fields":["aGk="]}""",
    };

    public static TheoryData<string> InvalidRepeatedScalarPayloads() => new()
    {
        """{"int32Fields":[null]}""",
        """{"stringFields":[null]}""",
        """{"boolFields":[null]}""",
        """{"bytesFields":[null]}""",
        """{"doubleFields":{}}""",
        """{"int32Fields":[2147483648]}""",
        """{"int32Fields":["x"]}""",
        """{"boolFields":["true"]}""",
        """{"stringFields":"a"}""",
    };

    public static TheoryData<string> EnumPayloads() => new()
    {
        "{}",
        """{"topLevelEnumField":"TOP_LEVEL_ENUM_FIRST"}""",
        """{"topLevelEnumField":1}""",
        """{"topLevelEnumField":"TOP_LEVEL_ENUM_NEGATIVE"}""",
        """{"topLevelEnumField":-1}""",
        """{"topLevelEnumField":"TOP_LEVEL_ENUM_UNSPECIFIED"}""",
        """{"topLevelEnumField":99}""",
        """{"topLevelEnumField":"TOP_LEVEL_ENUM_MISSING"}""",
        """{"topLevelEnumField":null}""",
        """{"nestedEnumField":"NESTED_ENUM_BETA"}""",
        """{"nestedEnumField":2}""",
        """{"topLevelEnumFields":["TOP_LEVEL_ENUM_FIRST",2,"TOP_LEVEL_ENUM_NEGATIVE"]}""",
        """{"topLevelEnumFields":[]}""",
        """{"topLevelEnumMap":{"a":"TOP_LEVEL_ENUM_SECOND","b":1,"c":-1}}""",
        """{"topLevelEnumMap":{}}""",
        """{"top_level_enum_field":"TOP_LEVEL_ENUM_SECOND","nested_enum_field":1}""",
        """{"topLevelEnumField":"TOP_LEVEL_ENUM_FIRST"}""",
        """{"nested_enum_field":"NESTED_ENUM_BETA","topLevelEnumField":"TOP_LEVEL_ENUM_MISSING"}""",
    };

    public static TheoryData<string> InvalidEnumPayloads() => new()
    {
        """{"topLevelEnumField":1.5}""",
        """{"topLevelEnumField":{}}""",
        """{"topLevelEnumFields":"TOP_LEVEL_ENUM_FIRST"}""",
        """{"topLevelEnumMap":[]}""",
    };

    public static TheoryData<string> MapKeyPayloads() => new()
    {
        "{}",
        """{"int32KeyMap":{"-2147483648":"min","0":"zero","2147483647":"max"}}""",
        """{"int64KeyMap":{"-9223372036854775808":"min","9223372036854775807":"max"}}""",
        """{"uint32KeyMap":{"0":"zero","4294967295":"max"}}""",
        """{"uint64KeyMap":{"18446744073709551615":"max"}}""",
        """{"sint32KeyMap":{"-1":"minus","1":"plus"}}""",
        """{"sint64KeyMap":{"-1":"minus","1":"plus"}}""",
        """{"fixed32KeyMap":{"7":"seven"}}""",
        """{"fixed64KeyMap":{"8":"eight"}}""",
        """{"sfixed32KeyMap":{"-9":"minus"}}""",
        """{"sfixed64KeyMap":{"-10":"minus"}}""",
        """{"boolKeyMap":{"true":"yes","false":"no"}}""",
        """{"stringKeyMap":{"":"empty","key":"value","é":"accent"}}""",
        """{"int32KeyMap":{},"stringKeyMap":{}}""",
        """{"int32KeyMap":null}""",
        """{"int32_key_map":{"1":"one"},"bool_key_map":{"true":"yes"}}""",
    };

    public static TheoryData<string> InvalidMapKeyPayloads() => new()
    {
        """{"int32KeyMap":{"notanumber":"a"}}""",
        """{"int32KeyMap":{"2147483648":"a"}}""",
        """{"uint32KeyMap":{"-1":"a"}}""",
        """{"boolKeyMap":{"yes":"a"}}""",
        """{"int32KeyMap":[]}""",
        """{"int32KeyMap":"a"}""",
    };

    public static TheoryData<string> MapValuePayloads() => new()
    {
        "{}",
        """{"doubleValueMap":{"a":1.5},"floatValueMap":{"a":2.5}}""",
        """{"doubleValueMap":{"a":"NaN"}}""",
        """{"int32ValueMap":{"a":-1},"int64ValueMap":{"a":"-2"}}""",
        """{"uint32ValueMap":{"a":1},"uint64ValueMap":{"a":"2"}}""",
        """{"sint32ValueMap":{"a":-3},"sint64ValueMap":{"a":"-4"}}""",
        """{"fixed32ValueMap":{"a":5},"fixed64ValueMap":{"a":"6"}}""",
        """{"sfixed32ValueMap":{"a":-7},"sfixed64ValueMap":{"a":"-8"}}""",
        """{"boolValueMap":{"a":true,"b":false}}""",
        """{"stringValueMap":{"a":"text"},"bytesValueMap":{"a":"aGk="}}""",
        """{"enumValueMap":{"a":"TOP_LEVEL_ENUM_FIRST","b":2,"c":-1}}""",
        """{"messageValueMap":{"a":{"leafStringField":"x"},"b":{}}}""",
        """{"int32ValueMap":{"a":"5"}}""",
        """{"message_value_map":{"a":{"leaf_int32_field":1}}}""",
    };

    public static TheoryData<string> InvalidMapValuePayloads() => new()
    {
        """{"int32ValueMap":{"a":"x"}}""",
        """{"messageValueMap":{"a":5}}""",
        """{"enumValueMap":{"a":1.5}}""",
    };

    public static TheoryData<string> MessagePayloads() => new()
    {
        "{}",
        """{"leafMessageField":{"leafStringField":"text","leafInt32Field":7}}""",
        """{"leafMessageField":{}}""",
        """{"leafMessageField":null}""",
        """{"nestedMessageField":{"nestedStringField":"text","nestedLeafMessageField":{"leafInt32Field":1}}}""",
        """{"leafMessageFields":[{"leafInt32Field":1},{},{"leafStringField":"x"}]}""",
        """{"leafMessageFields":[]}""",
        """{"leafMessageMap":{"a":{"leafInt32Field":1},"b":{}}}""",
        """{"leafMessageMap":{}}""",
        """{"leaf_message_field":{"leaf_string_field":"text"},"nested_message_field":{}}""",
        """{"leafMessageField":{"leafStringField":"text","unknownField":1},"unknownField":2}""",
    };

    public static TheoryData<string> InvalidMessagePayloads() => new()
    {
        """{"leafMessageField":5}""",
        """{"leafMessageField":"text"}""",
        """{"leafMessageField":[]}""",
        """{"leafMessageFields":{}}""",
        """{"leafMessageMap":[]}""",
        """{"leafMessageField":{"leafInt32Field":"x"}}""",
    };

    public static TheoryData<string> RecursivePayloads() => new()
    {
        """{"nameField":"root"}""",
        """{"nameField":"root","childField":{"nameField":"child"}}""",
        """{"nameField":"root","childField":{"childField":{"childField":{"nameField":"deep"}}}}""",
        """{"nameField":"root","childFields":[{"nameField":"a"},{"childField":{"nameField":"b"}}]}""",
    };

    public static TheoryData<string> OneofPayloads() => new()
    {
        "{}",
        """{"firstStringField":"text"}""",
        """{"firstInt32Field":7}""",
        """{"firstMessageField":{"leafStringField":"leaf"}}""",
        """{"secondBoolField":true}""",
        """{"secondBytesField":"aGk="}""",
        """{"firstStringField":"text","secondBoolField":false}""",
        """{"firstStringField":"text","plainStringField":"plain"}""",
        """{"firstStringField":""}""",
        """{"first_string_field":"text","second_bytes_field":"aGk="}""",
        """{"firstStringField":null}""",
    };

    public static TheoryData<string> InvalidOneofPayloads() => new()
    {
        """{"firstStringField":"text","firstInt32Field":1}""",
        """{"firstStringField":"text","firstMessageField":{}}""",
        """{"secondBoolField":true,"secondBytesField":"aGk="}""",
        """{"firstStringField":null,"firstInt32Field":1}""",
    };

    public static TheoryData<string> OptionalPayloads() => new()
    {
        "{}",
        """{"optionalInt32Field":0}""",
        """{"optionalInt32Field":7}""",
        """{"optionalStringField":""}""",
        """{"optionalStringField":"text"}""",
        """{"optionalBoolField":false}""",
        """{"optionalBytesField":""}""",
        """{"optionalMessageField":{}}""",
        """{"optionalMessageField":{"leafInt32Field":1}}""",
        """{"optionalInt32Field":null,"optionalStringField":null}""",
        """{"optional_int32_field":1,"optional_bool_field":true}""",
    };

    public static TheoryData<string> InvalidOptionalPayloads() => new()
    {
        """{"optionalInt32Field":"x"}""",
        """{"optionalMessageField":5}""",
        """{"optionalInt32Field":1,"optionalInt32Field":2}""",
    };

    public static TheoryData<string> WellKnownPayloads() => new()
    {
        "{}",
        """{"doubleValueField":1.5,"floatValueField":-2.5}""",
        """{"doubleValueField":"NaN","floatValueField":"Infinity"}""",
        """{"int64ValueField":"-9223372036854775808","uint64ValueField":"18446744073709551615"}""",
        """{"int32ValueField":-2147483648,"uint32ValueField":4294967295}""",
        """{"boolValueField":true,"stringValueField":"text","bytesValueField":"aGk="}""",
        """{"doubleValueField":null,"stringValueField":null,"boolValueField":null}""",
        """{"timestampField":"2026-09-12T10:00:00Z"}""",
        """{"timestampField":"2026-09-12T10:00:00.123456789+05:30"}""",
        """{"durationField":"1.500s"}""",
        """{"durationField":"-0.000000001s"}""",
        """{"durationField":"0s"}""",
        """{"durationField":"0.5s"}""",
        """{"durationField":"-1.5s"}""",
        """{"durationField":"1.000000001s"}""",
        """{"durationField":"315576000000s"}""",
        """{"durationField":"-315576000000s"}""",
        """{"fieldMaskField":"stringValueField,timestampField"}""",
        """{"fieldMaskField":""}""",
        """{"fieldMaskField":"stringValueField,timestampField"}""",
        """{"structField":{"a":1,"b":[true,null,"x"],"c":{"d":{}}}}""",
        """{"structField":{}}""",
        """{"valueField":null}""",
        """{"valueField":1.5}""",
        """{"valueField":"text"}""",
        """{"valueField":true}""",
        """{"valueField":[1,"two",null]}""",
        """{"valueField":{"nested":{"deep":[1]}}}""",
        """{"listValueField":[1,"two",null,{"a":1},[2]]}""",
        """{"listValueField":[]}""",
        $$$"""{"anyField":{"@type":"{{{TypeUrlPrefix}}}LeafMessage","leafStringField":"leaf"}}""",
        $$$"""{"anyField":{"leafStringField":"leaf","@type":"{{{TypeUrlPrefix}}}LeafMessage"}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Timestamp","value":"2026-09-12T10:00:00Z"}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Duration","value":"2s"}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Int32Value","value":7}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.StringValue","value":"text"}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Struct","value":{"a":1}}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.ListValue","value":[1,2]}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Value","value":"text"}}""",
        """{"emptyField":{}}""",
        """{"anyField":{"@type":"type.googleapis.com/google.protobuf.Empty","value":{}}}""",
        """{"nullValueField":null}""",
        """{"nullValueField":"NULL_VALUE"}""",
        """{"double_value_field":1.5,"timestamp_field":"2026-09-12T10:00:00Z","null_value_field":null}""",
    };

    public static TheoryData<string> InvalidWellKnownPayloads() => new()
    {
        """{"timestampField":"2026-09-12"}""",
        """{"timestampField":5}""",
        """{"durationField":"3.5"}""",
        """{"durationField":"1s1s"}""",
        """{"durationField":"+1s"}""",
        """{"durationField":".5s"}""",
        """{"durationField":"5.s"}""",
        """{"durationField":"1.1234567890s"}""",
        """{"durationField":"315576000001s"}""",
        """{"durationField":" 1s"}""",
        """{"durationField":"1S"}""",
        """{"durationField":"1e3s"}""",
        """{"durationField":"--1s"}""",
        """{"durationField":"s"}""",
        """{"int32ValueField":"x"}""",
        """{"structField":5}""",
        """{"listValueField":{}}""",
        """{"emptyField":5}""",
        """{"fieldMaskField":"string_value_field"}""",
        $$$"""{"anyField":{"@type":"{{{TypeUrlPrefix}}}MissingMessage"}}""",
    };

    public static TheoryData<string> WellKnownRepeatedPayloads() => new()
    {
        "{}",
        """{"doubleValueFields":[1.5,-2.5],"int32ValueFields":[1,2,3]}""",
        """{"stringValueFields":["a",""],"bytesValueFields":["aGk=",""]}""",
        """{"timestampFields":["2026-09-12T10:00:00Z","1970-01-01T00:00:00Z"]}""",
        """{"durationFields":["1s","-2.500s"]}""",
        """{"fieldMaskFields":["aB","c"]}""",
        """{"structFields":[{"a":1},{}]}""",
        """{"valueFields":[1,null,"x",[1],{"a":1}]}""",
        """{"listValueFields":[[1,2],[]]}""",
        $$"""{"anyFields":[{"@type":"{{TypeUrlPrefix}}LeafMessage","leafInt32Field":1}]}""",
        """{"emptyFields":[{},{}]}""",
        """{"nullValueFields":[null,null]}""",
        """{"doubleValueFields":[],"timestampFields":[]}""",
    };

    public static TheoryData<string> WellKnownMapPayloads() => new()
    {
        "{}",
        """{"doubleValueMap":{"a":1.5},"int32ValueMap":{"a":2}}""",
        """{"stringValueMap":{"a":"text"},"boolValueMap":{"a":true}}""",
        """{"timestampMap":{"a":"2026-09-12T10:00:00Z"}}""",
        """{"durationMap":{"a":"1s","b":"-2.5s"}}""",
        """{"fieldMaskMap":{"a":"aB"}}""",
        """{"structMap":{"a":{"k":"v"}}}""",
        """{"valueMap":{"a":null,"b":1,"c":[1]}}""",
        """{"listValueMap":{"a":[1,2]}}""",
        "{\"anyMap\":{\"a\":{\"@type\":\"" + TypeUrlPrefix + "LeafMessage\",\"leafInt32Field\":1}}}",
        """{"emptyMap":{"a":{}}}""",
        """{"doubleValueMap":{},"timestampMap":{}}""",
    };

    public static TheoryData<string> TimestampValues() => new()
    {
        "2026-09-12T10:00:00Z",
        "2026-09-12T10:00:00.021Z",
        "2026-09-12T15:30:00+05:30",
        "1969-07-20T20:17:40.000000001-07:00",
        "0001-01-01T00:00:00Z",
        "9999-12-31T23:59:59.999999999Z",
    };

    public static TheoryData<string> InvalidTimestampValues() => new()
    {
        "2023-02-29T00:00:00Z",
        "2026-13-01T00:00:00Z",
        "2026-00-01T00:00:00Z",
        "2026-09-00T00:00:00Z",
        "2026-09-12T24:00:00Z",
        "2026-09-12T10:60:00Z",
        "2026-09-12T10:00:60Z",
        "0000-01-01T00:00:00Z",
        "2026-09-12T10:00:00",
        "2026-09-12T10:00:00z",
        "2026-09-12 10:00:00Z",
        "2026-09-12T10:00:00.Z",
        "2026-09-12T10:00:00.1234567890Z",
        "2026-09-12T10:00:00+0530",
        "2026-09-12T10:00:00+24:00",
        "2026-09-12T10:00:00Z ",
        "226-09-12T10:00:00Z",
        "",
    };

    public static TheoryData<string> DoubleLiterals() => new()
    {
        "0",
        "-0",
        "1",
        "0.1",
        "1.5",
        "-2.25",
        "3.141592653589793",
        "9007199254740991",
        "9007199254740992",
        "9007199254740993",
        "1e22",
        "1e23",
        "1e-22",
        "1e-23",
        "1.7976931348623157e308",
        "5e-324",
        "2.2250738585072011e-308",
        "1.0000000000000002",
        "123456789012345678901234567890",
        "0.000000000000000000001",
        "1E+2",
        "1e-0",
        "-1.23e-7",
    };

    public static TheoryData<string> FloatLiterals() => new()
    {
        "1.5",
        "0.1",
        "3.4028235e38",
        "1.1754944e-38",
    };

    public static TheoryData<string> Utf8StringValues() => new()
    {
        "",
        "a",
        "ascii value",
        "123456789012345",
        "1234567890123456",
        "12345678901234567",
        "0123456789012345678901234567890123456789",
        "café",
        "ünïcödé value here",
        "日本語のテキスト",
        "emoji 🙂 tail",
        "🙂",
        "123456789012345🙂",
        "ascii then 日本語",
        "\u0000control\u001f",
    };
}
