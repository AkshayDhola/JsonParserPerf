using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class WellKnownTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.WellKnownPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_WellKnownPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<WellKnownTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<WellKnownTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<WellKnownTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidWellKnownPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidWellKnownPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<WellKnownTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<WellKnownTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_AnyWithoutTypeUrl_ShouldThrowWhereGoogleReturnsEmptyAny()
    {
        // Arrange
        const string json = """{"anyField":{"leafStringField":"leaf"}}""";
        var google = TestFactory.GoogleParser.Parse<WellKnownTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<WellKnownTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.AnyField.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize_EveryWellKnownTypeSet_ShouldPopulateEveryField()
    {
        // Arrange
        var json = $$"""
            {
              "doubleValueField": 1.5, "floatValueField": 2.5, "int64ValueField": "-3",
              "uint64ValueField": "4", "int32ValueField": -5, "uint32ValueField": 6,
              "boolValueField": true, "stringValueField": "text", "bytesValueField": "aGk=",
              "timestampField": "2026-09-12T10:00:00Z", "durationField": "1.500s",
              "fieldMaskField": "stringValueField,timestampField",
              "structField": {"k": "v"}, "valueField": true, "listValueField": [1, 2],
              "anyField": {"@type": "{{TestFactory.TypeUrlPrefix}}LeafMessage", "leafStringField": "leaf"},
              "emptyField": {}, "nullValueField": null
            }
            """;
        var expected = TestFactory.GoogleParser.Parse<WellKnownTypes>(json);

        // Act
        var message = TestFactory.Deserialize<WellKnownTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.DoubleValueField.Should().Be(1.5);
        message.FloatValueField.Should().Be(2.5f);
        message.Int64ValueField.Should().Be(-3L);
        message.Uint64ValueField.Should().Be(4UL);
        message.Int32ValueField.Should().Be(-5);
        message.Uint32ValueField.Should().Be(6U);
        message.BoolValueField.Should().BeTrue();
        message.StringValueField.Should().Be("text");
        message.BytesValueField.ToStringUtf8().Should().Be("hi");
        message.TimestampField.ToDateTime().Should().Be(new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        message.DurationField.ToTimeSpan().Should().Be(TimeSpan.FromSeconds(1.5));
        message.FieldMaskField.Paths.Should().Equal("string_value_field", "timestamp_field");
        message.StructField.Fields["k"].StringValue.Should().Be("v");
        message.ValueField.BoolValue.Should().BeTrue();
        message.ListValueField.Values.Should().HaveCount(2);
        message.AnyField.Unpack<LeafMessage>().LeafStringField.Should().Be("leaf");
        message.EmptyField.Should().NotBeNull();
        message.NullValueField.Should().Be(NullValue.NullValue);
    }

    [Theory]
    [MemberData(nameof(TestFactory.WellKnownRepeatedPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_WellKnownRepeatedPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<WellKnownRepeatedTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<WellKnownRepeatedTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<WellKnownRepeatedTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_RepeatedWrappers_ShouldReadBareValues()
    {
        // Arrange
        const string json = """{"int32ValueFields":[1,2],"stringValueFields":["a","b"],"doubleValueFields":[1.5]}""";
        var expected = TestFactory.GoogleParser.Parse<WellKnownRepeatedTypes>(json);

        // Act
        var message = TestFactory.Deserialize<WellKnownRepeatedTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.Int32ValueFields.Should().Equal(1, 2);
        message.StringValueFields.Should().Equal("a", "b");
        message.DoubleValueFields.Should().Equal(1.5);
    }

    [Theory]
    [MemberData(nameof(TestFactory.WellKnownMapPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_WellKnownMapPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<WellKnownMapTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<WellKnownMapTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<WellKnownMapTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_WrapperMapValues_ShouldReadBareValues()
    {
        // Arrange
        const string json = """{"int32ValueMap":{"a":1},"stringValueMap":{"a":"text"},"boolValueMap":{"a":false}}""";
        var expected = TestFactory.GoogleParser.Parse<WellKnownMapTypes>(json);

        // Act
        var message = TestFactory.Deserialize<WellKnownMapTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.Int32ValueMap["a"].Should().Be(1);
        message.StringValueMap["a"].Should().Be("text");
        message.BoolValueMap["a"].Should().BeFalse();
    }

    [Theory]
    [InlineData("\"text\"")]
    [InlineData("1.5")]
    [InlineData("true")]
    [InlineData("{\"a\":1}")]
    [InlineData("[1,\"two\",null]")]
    [InlineData("{}")]
    public void Deserialize_ValueAtTopLevel_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<Value>(json);

        // Act
        var value = TestFactory.Deserialize<Value>(json);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_DurationAndTimestampAtTopLevel_ShouldMatchGoogleParser()
    {
        // Arrange
        var expectedDuration = TestFactory.GoogleParser.Parse<Duration>("\"5s\"");
        var expectedTimestamp = TestFactory.GoogleParser.Parse<Timestamp>("\"2026-09-12T10:00:00Z\"");

        // Act
        var duration = TestFactory.Deserialize<Duration>("\"5s\"");
        var timestamp = TestFactory.Deserialize<Timestamp>("\"2026-09-12T10:00:00Z\"");

        // Assert
        duration.Should().Be(expectedDuration);
        timestamp.Should().Be(expectedTimestamp);
    }

    [Theory]
    [InlineData("7")]
    [InlineData("-2147483648")]
    public void Deserialize_Int32ValueAtTopLevel_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<Int32Value>(json);

        // Act
        var message = TestFactory.Deserialize<Int32Value>(json);

        // Assert
        message.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"x\"")]
    [InlineData("\"\"")]
    public void Deserialize_StringValueAtTopLevel_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<StringValue>(json);

        // Act
        var message = TestFactory.Deserialize<StringValue>(json);

        // Assert
        message.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_BoolAndBytesAndDoubleWrappersAtTopLevel_ShouldMatchGoogleParser()
    {
        // Arrange
        var expectedBool = TestFactory.GoogleParser.Parse<BoolValue>("true");
        var expectedBytes = TestFactory.GoogleParser.Parse<BytesValue>("\"aGk=\"");
        var expectedDouble = TestFactory.GoogleParser.Parse<DoubleValue>("1.5");

        // Act
        var boolValue = TestFactory.Deserialize<BoolValue>("true");
        var bytesValue = TestFactory.Deserialize<BytesValue>("\"aGk=\"");
        var doubleValue = TestFactory.Deserialize<DoubleValue>("1.5");

        // Assert
        boolValue.Should().Be(expectedBool);
        bytesValue.Should().Be(expectedBytes);
        doubleValue.Should().Be(expectedDouble);
    }

    [Fact]
    public void Deserialize_StructListValueFieldMaskAndEmptyAtTopLevel_ShouldMatchGoogleParser()
    {
        // Arrange
        var expectedStruct = TestFactory.GoogleParser.Parse<Struct>("""{"a":1}""");
        var expectedList = TestFactory.GoogleParser.Parse<ListValue>("[1,2]");
        var expectedMask = TestFactory.GoogleParser.Parse<FieldMask>("\"aB\"");
        var expectedEmpty = TestFactory.GoogleParser.Parse<Empty>("{}");

        // Act
        var structValue = TestFactory.Deserialize<Struct>("""{"a":1}""");
        var listValue = TestFactory.Deserialize<ListValue>("[1,2]");
        var mask = TestFactory.Deserialize<FieldMask>("\"aB\"");
        var empty = TestFactory.Deserialize<Empty>("{}");

        // Assert
        structValue.Should().Be(expectedStruct);
        listValue.Should().Be(expectedList);
        mask.Should().Be(expectedMask);
        empty.Should().Be(expectedEmpty);
    }

    [Fact]
    public void Deserialize_TopLevelNullAsValue_ShouldMatchGoogleNullValue()
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<Value>("null");

        // Act
        var value = TestFactory.Deserialize<Value>("null");

        // Assert
        value.Should().Be(expected);
    }
}
