using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Tests.UnitTests;

public class StructReadersTests
{
    [Fact]
    public void StructReader_ObjectWithMixedMembers_ShouldReturnStruct()
    {
        // Arrange
        const string json = """{"a":1,"b":[true,null,"x"],"c":{"d":{}}}""";

        // Act
        var result = TestFactory.ReadValue<StructReader, Struct>(json);

        // Assert
        result.Fields["a"].NumberValue.Should().Be(1);
        result.Fields["b"].ListValue.Values.Should().Equal(Value.ForBool(true), Value.ForNull(), Value.ForString("x"));
        result.Fields["c"].StructValue.Fields["d"].StructValue.Fields.Should().BeEmpty();
    }

    [Fact]
    public void StructReader_NonObjectToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<StructReader, Struct>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected an object*");
    }

    [Fact]
    public void StructReader_NestingAboveRecursionLimit_ShouldThrowJsonException()
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: 2);
        const string json = """{"a":{"b":{"c":{}}}}""";

        // Act
        var act = () => TestFactory.ReadValue<StructReader, Struct>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*recursion limit*");
    }

    [Fact]
    public void ListValueReader_ArrayOfMixedValues_ShouldReturnListValue()
    {
        // Arrange
        const string json = """[1,"two",null,{"a":1},[2]]""";

        // Act
        var result = TestFactory.ReadValue<ListValueReader, ListValue>(json);

        // Assert
        result.Values.Should().HaveCount(5);
        result.Values[1].StringValue.Should().Be("two");
        result.Values[4].ListValue.Values.Should().ContainSingle();
    }

    [Fact]
    public void ListValueReader_NonArrayToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "{}";

        // Act
        var act = () => TestFactory.ReadValue<ListValueReader, ListValue>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected an array*");
    }

    [Fact]
    public void ListValueReader_NestingAboveRecursionLimit_ShouldThrowJsonException()
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: 4);
        var json = TestFactory.NestedArrayJson(10);

        // Act
        var act = () => TestFactory.ReadValue<ListValueReader, ListValue>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*recursion limit*");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("1.5")]
    [InlineData("\"text\"")]
    [InlineData("{\"a\":1}")]
    [InlineData("[1]")]
    public void ProtobufValueReader_AnyJsonValue_ShouldReturnMatchingValueKind(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<Value>(json);

        // Act
        var value = TestFactory.ReadValue<ProtobufValueReader, Value>(json);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void EmptyReader_ObjectWithMembers_ShouldReturnEmpty()
    {
        // Arrange
        const string json = """{"ignored":[1,2]}""";

        // Act
        var result = TestFactory.ReadValue<EmptyReader, Empty>(json);

        // Assert
        result.Should().Be(new Empty());
    }

    [Fact]
    public void EmptyReader_NonObjectToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<EmptyReader, Empty>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected an object*");
    }
}
