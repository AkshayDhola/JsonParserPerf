using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class RecursionLimitParityTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void Deserialize_NestingAboveRecursionLimit_ShouldThrowJsonException(int limit)
    {
        // Arrange
        var options = TestFactory.SerializerOptions(TestFactory.ParserOptions(recursionLimit: limit));
        var json = TestFactory.NestedRecursiveJson(limit + 2);

        // Act
        var act = () => TestFactory.Deserialize<RecursiveMessage>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage($"*recursion limit of {limit}*");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(12)]
    public void Deserialize_NestingWithinRecursionLimit_ShouldMatchGoogleParser(int limit)
    {
        // Arrange
        var options = TestFactory.SerializerOptions(TestFactory.ParserOptions(recursionLimit: limit));
        var json = TestFactory.NestedRecursiveJson(limit);
        var expected = TestFactory.GoogleParser.Parse<RecursiveMessage>(json);

        // Act
        var message = TestFactory.Deserialize<RecursiveMessage>(json, options);

        // Assert
        message.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_StructNestingAboveRecursionLimit_ShouldThrowJsonException()
    {
        // Arrange
        var options = TestFactory.SerializerOptions(TestFactory.ParserOptions(recursionLimit: 4));
        var json = """{"structField":{"a":""" + TestFactory.NestedArrayJson(10) + "}}";

        // Act
        var act = () => TestFactory.Deserialize<WellKnownTypes>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*recursion limit of 4*");
    }
}
