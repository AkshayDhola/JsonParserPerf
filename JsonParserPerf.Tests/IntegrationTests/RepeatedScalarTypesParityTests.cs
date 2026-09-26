using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class RepeatedScalarTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.RepeatedScalarPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_RepeatedScalarPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<RepeatedScalarTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<RepeatedScalarTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<RepeatedScalarTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidRepeatedScalarPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidRepeatedScalarPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<RepeatedScalarTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<RepeatedScalarTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_AllRepeatedScalarFieldsSet_ShouldPopulateEveryField()
    {
        // Arrange
        const string json = """
            {
              "doubleFields": [1.5], "floatFields": [2.5], "int32Fields": [-3], "int64Fields": ["-4"],
              "uint32Fields": [5], "uint64Fields": ["6"], "sint32Fields": [-7], "sint64Fields": ["-8"],
              "fixed32Fields": [9], "fixed64Fields": ["10"], "sfixed32Fields": [-11], "sfixed64Fields": ["-12"],
              "boolFields": [true, false], "stringFields": ["a", "b"], "bytesFields": ["aGk="]
            }
            """;
        var expected = TestFactory.GoogleParser.Parse<RepeatedScalarTypes>(json);

        // Act
        var message = TestFactory.Deserialize<RepeatedScalarTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.DoubleFields.Should().Equal(1.5);
        message.Int64Fields.Should().Equal(-4L);
        message.Uint32Fields.Should().Equal(5U);
        message.Sint32Fields.Should().Equal(-7);
        message.Sfixed64Fields.Should().Equal(-12L);
        message.BoolFields.Should().Equal(true, false);
        message.StringFields.Should().Equal("a", "b");
        message.BytesFields.Should().ContainSingle();
    }
}
