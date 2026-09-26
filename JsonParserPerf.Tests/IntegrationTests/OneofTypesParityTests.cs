using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class OneofTypesParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.OneofPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_OneofPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<OneofTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<OneofTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<OneofTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidOneofPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_TwoMembersOfOneOneof_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<OneofTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<OneofTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData("""{"firstStringField":"text"}""", OneofTypes.FirstChoiceOneofCase.FirstStringField)]
    [InlineData("""{"firstInt32Field":1}""", OneofTypes.FirstChoiceOneofCase.FirstInt32Field)]
    [InlineData("""{"firstMessageField":{}}""", OneofTypes.FirstChoiceOneofCase.FirstMessageField)]
    public void Deserialize_SingleOneofMember_ShouldSetMatchingCase(
        string json,
        OneofTypes.FirstChoiceOneofCase expected)
    {
        // Arrange
        var google = TestFactory.GoogleParser.Parse<OneofTypes>(json);

        // Act
        var message = TestFactory.Deserialize<OneofTypes>(json)!;

        // Assert
        message.FirstChoiceCase.Should().Be(expected);
        message.Should().Be(google);
    }

    [Theory]
    [MemberData(nameof(TestFactory.OptionalPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_OptionalPayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<OptionalTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<OptionalTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<OptionalTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidOptionalPayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidOptionalPayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<OptionalTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<OptionalTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }
}
