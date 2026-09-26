using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class MapTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.MapKeyPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_MapKeyPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<MapKeyTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<MapKeyTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<MapKeyTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidMapKeyPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidMapKeyPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<MapKeyTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<MapKeyTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Theory]
    [MemberData(nameof(TestFactory.MapValuePayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_MapValuePayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<MapValueTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<MapValueTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<MapValueTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidMapValuePayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidMapValuePayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<MapValueTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<MapValueTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_EveryLegalMapKeyType_ShouldPopulateEveryMap()
    {
        // Arrange
        const string json = """
            {
              "int32KeyMap": {"-1": "a"}, "int64KeyMap": {"-2": "b"},
              "uint32KeyMap": {"3": "c"}, "uint64KeyMap": {"4": "d"},
              "sint32KeyMap": {"-5": "e"}, "sint64KeyMap": {"-6": "f"},
              "fixed32KeyMap": {"7": "g"}, "fixed64KeyMap": {"8": "h"},
              "sfixed32KeyMap": {"-9": "i"}, "sfixed64KeyMap": {"-10": "j"},
              "boolKeyMap": {"true": "k"}, "stringKeyMap": {"l": "l"}
            }
            """;
        var expected = TestFactory.GoogleParser.Parse<MapKeyTypes>(json);

        // Act
        var message = TestFactory.Deserialize<MapKeyTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.Int32KeyMap[-1].Should().Be("a");
        message.Int64KeyMap[-2L].Should().Be("b");
        message.Uint32KeyMap[3U].Should().Be("c");
        message.Uint64KeyMap[4UL].Should().Be("d");
        message.Sint32KeyMap[-5].Should().Be("e");
        message.Sint64KeyMap[-6L].Should().Be("f");
        message.Fixed32KeyMap[7U].Should().Be("g");
        message.Fixed64KeyMap[8UL].Should().Be("h");
        message.Sfixed32KeyMap[-9].Should().Be("i");
        message.Sfixed64KeyMap[-10L].Should().Be("j");
        message.BoolKeyMap[true].Should().Be("k");
        message.StringKeyMap["l"].Should().Be("l");
    }

    [Fact]
    public void Deserialize_EveryMapValueType_ShouldPopulateEveryMap()
    {
        // Arrange
        const string json = """
            {
              "doubleValueMap": {"k": 1.5}, "floatValueMap": {"k": 2.5},
              "int32ValueMap": {"k": -3}, "int64ValueMap": {"k": "-4"},
              "uint32ValueMap": {"k": 5}, "uint64ValueMap": {"k": "6"},
              "sint32ValueMap": {"k": -7}, "sint64ValueMap": {"k": "-8"},
              "fixed32ValueMap": {"k": 9}, "fixed64ValueMap": {"k": "10"},
              "sfixed32ValueMap": {"k": -11}, "sfixed64ValueMap": {"k": "-12"},
              "boolValueMap": {"k": true}, "stringValueMap": {"k": "text"},
              "bytesValueMap": {"k": "aGk="}, "enumValueMap": {"k": "TOP_LEVEL_ENUM_SECOND"},
              "messageValueMap": {"k": {"leafInt32Field": 13}}
            }
            """;
        var expected = TestFactory.GoogleParser.Parse<MapValueTypes>(json);

        // Act
        var message = TestFactory.Deserialize<MapValueTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.DoubleValueMap["k"].Should().Be(1.5);
        message.Int64ValueMap["k"].Should().Be(-4L);
        message.Uint64ValueMap["k"].Should().Be(6UL);
        message.Sint32ValueMap["k"].Should().Be(-7);
        message.Sfixed64ValueMap["k"].Should().Be(-12L);
        message.StringValueMap["k"].Should().Be("text");
        message.BytesValueMap["k"].ToStringUtf8().Should().Be("hi");
        message.EnumValueMap["k"].Should().Be(TopLevelEnum.Second);
        message.MessageValueMap["k"].LeafInt32Field.Should().Be(13);
    }
}
