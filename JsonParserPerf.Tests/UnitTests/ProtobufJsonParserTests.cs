using System.Text.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class ProtobufJsonParserTests
{
    [Fact]
    public void ParseObject_JsonNames_ShouldPopulateFields()
    {
        // Arrange
        const string json = """{"leafStringField":"text","leafInt32Field":7}""";

        // Act
        var message = TestFactory.ParseObject<LeafMessage>(json);

        // Assert
        message.LeafStringField.Should().Be("text");
        message.LeafInt32Field.Should().Be(7);
    }

    [Fact]
    public void ParseObject_ProtoNames_ShouldPopulateFields()
    {
        // Arrange
        const string json = """{"leaf_string_field":"text","leaf_int32_field":7}""";

        // Act
        var message = TestFactory.ParseObject<LeafMessage>(json);

        // Assert
        message.LeafStringField.Should().Be("text");
        message.LeafInt32Field.Should().Be(7);
    }

    [Fact]
    public void ParseObject_FieldsOutOfDeclarationOrder_ShouldPopulateFields()
    {
        // Arrange
        const string json = """{"leafInt32Field":7,"leafStringField":"text"}""";

        // Act
        var message = TestFactory.ParseObject<LeafMessage>(json);

        // Assert
        message.LeafStringField.Should().Be("text");
        message.LeafInt32Field.Should().Be(7);
    }

    [Fact]
    public void ParseObject_UnknownField_ShouldSkipIt()
    {
        // Arrange
        const string json = """{"unknownField":{"nested":[1,2]},"leafInt32Field":7}""";

        // Act
        var message = TestFactory.ParseObject<LeafMessage>(json);

        // Assert
        message.Should().Be(new LeafMessage { LeafInt32Field = 7 });
    }

    [Fact]
    public void ParseObject_NullField_ShouldLeaveDefault()
    {
        // Arrange
        const string json = """{"leafMessageField":null,"leafMessageFields":null,"leafMessageMap":null}""";

        // Act
        var message = TestFactory.ParseObject<MessageTypes>(json);

        // Assert
        message.Should().Be(new MessageTypes());
    }

    [Fact]
    public void ParseObject_NestedRepeatedAndMapMessages_ShouldPopulateEveryLevel()
    {
        // Arrange
        const string json = """
            {
              "leafMessageField": {"leafStringField": "leaf"},
              "nestedMessageField": {"nestedLeafMessageField": {"leafInt32Field": 2}},
              "leafMessageFields": [{"leafInt32Field": 3}, {"leafInt32Field": 4}],
              "leafMessageMap": {"key": {"leafStringField": "mapped"}}
            }
            """;

        // Act
        var message = TestFactory.ParseObject<MessageTypes>(json);

        // Assert
        message.LeafMessageField.LeafStringField.Should().Be("leaf");
        message.NestedMessageField.NestedLeafMessageField.LeafInt32Field.Should().Be(2);
        message.LeafMessageFields.Select(item => item.LeafInt32Field).Should().Equal(3, 4);
        message.LeafMessageMap["key"].LeafStringField.Should().Be("mapped");
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("5")]
    [InlineData("\"text\"")]
    [InlineData("true")]
    public void ParseObject_NonObjectToken_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ParseObject<LeafMessage>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected an object*");
    }

    [Theory]
    [InlineData("""{"firstStringField":"text","firstInt32Field":1}""")]
    [InlineData("""{"firstStringField":null,"firstInt32Field":1}""")]
    [InlineData("""{"secondBoolField":true,"secondBytesField":"aGk="}""")]
    public void ParseObject_TwoMembersOfOneOneof_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ParseObject<OneofTypes>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*Multiple values*oneof*");
    }

    [Fact]
    public void ParseObject_MembersOfDifferentOneofs_ShouldSetBothCases()
    {
        // Arrange
        const string json = """{"firstInt32Field":1,"secondBoolField":true}""";

        // Act
        var message = TestFactory.ParseObject<OneofTypes>(json);

        // Assert
        message.FirstChoiceCase.Should().Be(OneofTypes.FirstChoiceOneofCase.FirstInt32Field);
        message.SecondChoiceCase.Should().Be(OneofTypes.SecondChoiceOneofCase.SecondBoolField);
    }

    [Fact]
    public void ParseObject_ExplicitDefaultsForOptionalFields_ShouldMarkPresence()
    {
        // Arrange
        const string json =
            """{"optionalInt32Field":0,"optionalStringField":"","optionalBoolField":false,"optionalBytesField":""}""";

        // Act
        var message = TestFactory.ParseObject<OptionalTypes>(json);

        // Assert
        message.HasOptionalInt32Field.Should().BeTrue();
        message.HasOptionalStringField.Should().BeTrue();
        message.HasOptionalBoolField.Should().BeTrue();
        message.HasOptionalBytesField.Should().BeTrue();
    }

    [Fact]
    public void ParseObject_EmptyObjectForOptionalFields_ShouldLeavePresenceUnset()
    {
        // Arrange
        const string json = "{}";

        // Act
        var message = TestFactory.ParseObject<OptionalTypes>(json);

        // Assert
        message.HasOptionalInt32Field.Should().BeFalse();
        message.HasOptionalStringField.Should().BeFalse();
        message.OptionalMessageField.Should().BeNull();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void ParseObject_NestingAboveRecursionLimit_ShouldThrowJsonException(int limit)
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: limit);
        var json = TestFactory.NestedRecursiveJson(limit + 2);

        // Act
        var act = () => TestFactory.ParseObject<RecursiveMessage>(json, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage($"*recursion limit of {limit}*");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(12)]
    public void ParseObject_NestingWithinRecursionLimit_ShouldParseEveryLevel(int limit)
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: limit);
        var json = TestFactory.NestedRecursiveJson(limit);

        // Act
        var message = TestFactory.ParseObject<RecursiveMessage>(json, options);

        // Assert
        var depth = 0;
        for (var node = message; node is not null; node = node.ChildField)
        {
            depth++;
        }

        depth.Should().Be(limit);
    }

    [Theory]
    [InlineData("""{"leafMessageField":{"leafInt32Field":"x"}}""", "int32")]
    [InlineData("""{"leafMessageField":{"leafStringField":5}}""", "String")]
    public void ParseObject_InvalidNestedValue_ShouldNameFailingType(string json, string expectedType)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ParseObject<MessageTypes>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage($"*{expectedType}*");
    }
}
