using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class EnumTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.EnumPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_EnumPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<EnumTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<EnumTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<EnumTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidEnumPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidEnumPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<EnumTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<EnumTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_RepeatedAndMapEnums_ShouldPopulateBoth()
    {
        // Arrange
        const string json = """{"topLevelEnumFields":["TOP_LEVEL_ENUM_FIRST",2],"topLevelEnumMap":{"a":-1}}""";
        var expected = TestFactory.GoogleParser.Parse<EnumTypes>(json);

        // Act
        var message = TestFactory.Deserialize<EnumTypes>(json)!;

        // Assert
        message.Should().Be(expected);
        message.TopLevelEnumFields.Should().Equal(TopLevelEnum.First, TopLevelEnum.Second);
        message.TopLevelEnumMap["a"].Should().Be(TopLevelEnum.Negative);
    }
}
