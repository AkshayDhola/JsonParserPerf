using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class ScalarTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.ScalarPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_ScalarPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<ScalarTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<ScalarTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<ScalarTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidScalarPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidScalarPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<ScalarTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<ScalarTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_AllScalarFieldsSet_ShouldPopulateEveryField()
    {
        // Arrange
        const string json = """
            {
              "doubleField": 1.5, "floatField": 2.5,
              "int32Field": -3, "int64Field": "-4",
              "uint32Field": 5, "uint64Field": "6",
              "sint32Field": -7, "sint64Field": "-8",
              "fixed32Field": 9, "fixed64Field": "10",
              "sfixed32Field": -11, "sfixed64Field": "-12",
              "boolField": true, "stringField": "text", "bytesField": "aGk="
            }
            """;
        var expected = TestFactory.GoogleParser.Parse<ScalarTypes>(json);

        // Act
        var message = TestFactory.Deserialize<ScalarTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.DoubleField.Should().Be(1.5);
        message.FloatField.Should().Be(2.5f);
        message.Int32Field.Should().Be(-3);
        message.Int64Field.Should().Be(-4L);
        message.Uint32Field.Should().Be(5U);
        message.Uint64Field.Should().Be(6UL);
        message.Sint32Field.Should().Be(-7);
        message.Sint64Field.Should().Be(-8L);
        message.Fixed32Field.Should().Be(9U);
        message.Fixed64Field.Should().Be(10UL);
        message.Sfixed32Field.Should().Be(-11);
        message.Sfixed64Field.Should().Be(-12L);
        message.BoolField.Should().BeTrue();
        message.StringField.Should().Be("text");
        message.BytesField.ToStringUtf8().Should().Be("hi");
    }

    [Theory]
    [InlineData("""{"stringField":"text" """)]
    [InlineData("""{"int32Fields":[1 """)]
    [InlineData("""{"stringKeyMap":{"a":"b" """)]
    [InlineData("{")]
    [InlineData("")]
    public void Deserialize_TruncatedJson_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.Deserialize<ScalarTypes>(input);

        // Assert
        act.Should().Throw<JsonException>();
    }
}
