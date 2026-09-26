using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Tests.UnitTests;

public class FieldMaskReaderTests
{
    [Fact]
    public void Read_CamelCasePaths_ShouldReturnSnakeCasePaths()
    {
        // Arrange
        const string json = "\"stringValueField,timestampField,plain\"";

        // Act
        var mask = TestFactory.ReadValue<FieldMaskReader, FieldMask>(json);

        // Assert
        mask.Paths.Should().Equal("string_value_field", "timestamp_field", "plain");
    }

    [Fact]
    public void Read_EmptyString_ShouldReturnNoPaths()
    {
        // Arrange
        const string json = "\"\"";

        // Act
        var mask = TestFactory.ReadValue<FieldMaskReader, FieldMask>(json);

        // Assert
        mask.Paths.Should().BeEmpty();
    }

    [Fact]
    public void Read_EscapedPath_ShouldUnescapeBeforeConverting()
    {
        // Arrange
        const string json = "\"string\\u0056alueField\"";

        // Act
        var mask = TestFactory.ReadValue<FieldMaskReader, FieldMask>(json);

        // Assert
        mask.Paths.Should().Equal("string_value_field");
    }

    [Fact]
    public void Read_SnakeCasePath_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "\"string_value_field\"";

        // Act
        var act = () => TestFactory.ReadValue<FieldMaskReader, FieldMask>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*camelCase*");
    }

    [Fact]
    public void Read_NumberToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<FieldMaskReader, FieldMask>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected a string*");
    }
}
