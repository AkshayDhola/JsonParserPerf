using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.Values;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class EnumReaderTests
{
    [Theory]
    [InlineData("\"TOP_LEVEL_ENUM_SECOND\"", TopLevelEnum.Second)]
    [InlineData("2", TopLevelEnum.Second)]
    [InlineData("\"TOP_LEVEL_ENUM_NEGATIVE\"", TopLevelEnum.Negative)]
    [InlineData("-1", TopLevelEnum.Negative)]
    public void Read_NameOrNumber_ShouldReturnEnumValue(string json, TopLevelEnum expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<EnumReader<TopLevelEnum>, TopLevelEnum>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void Read_EscapedName_ShouldReturnEnumValue()
    {
        // Arrange
        const string json = "\"TOP_LEVEL_ENUM_\\u0046IRST\"";

        // Act
        var value = TestFactory.ReadValue<EnumReader<TopLevelEnum>, TopLevelEnum>(json);

        // Assert
        value.Should().Be(TopLevelEnum.First);
    }

    [Fact]
    public void Read_NestedEnumName_ShouldReturnNestedEnumValue()
    {
        // Arrange
        const string json = "\"NESTED_ENUM_ALPHA\"";

        // Act
        var value = TestFactory.ReadValue<EnumReader<EnumTypes.Types.NestedEnum>, EnumTypes.Types.NestedEnum>(json);

        // Assert
        value.Should().Be(EnumTypes.Types.NestedEnum.Alpha);
    }

    [Fact]
    public void Read_UnknownName_ShouldReturnDefault()
    {
        // Arrange
        const string json = "\"TOP_LEVEL_ENUM_MISSING\"";

        // Act
        var value = TestFactory.ReadValue<EnumReader<TopLevelEnum>, TopLevelEnum>(json);

        // Assert
        value.Should().Be(TopLevelEnum.Unspecified);
    }

    [Fact]
    public void Read_UnknownNumber_ShouldKeepNumber()
    {
        // Arrange
        const string json = "99";

        // Act
        var value = TestFactory.ReadValue<EnumReader<TopLevelEnum>, TopLevelEnum>(json);

        // Assert
        ((int)value).Should().Be(99);
    }

    [Fact]
    public void Read_NullForNullValue_ShouldReturnNullValue()
    {
        // Arrange
        const string json = "null";

        // Act
        var value = TestFactory.ReadValue<EnumReader<NullValue>, NullValue>(json);

        // Assert
        value.Should().Be(NullValue.NullValue);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("null")]
    [InlineData("{}")]
    public void Read_InvalidToken_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<EnumReader<TopLevelEnum>, TopLevelEnum>(input);

        // Assert
        act.Should().Throw<JsonException>();
    }
}
