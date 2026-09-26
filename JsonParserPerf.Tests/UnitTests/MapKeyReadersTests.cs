using System.Text.Json;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Tests.UnitTests;

public class MapKeyReadersTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BoolKeyReader_BooleanKey_ShouldReturnBool(string key, bool expected)
    {
        // Arrange
        var input = key;

        // Act
        var value = TestFactory.ReadKey<BoolKeyReader, bool>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("True")]
    [InlineData("")]
    public void BoolKeyReader_NonBooleanKey_ShouldThrowJsonException(string key)
    {
        // Arrange
        var input = key;

        // Act
        var act = () => TestFactory.ReadKey<BoolKeyReader, bool>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*bool*");
    }

    [Theory]
    [InlineData("key")]
    [InlineData("kéy")]
    [InlineData("🙂")]
    [InlineData("")]
    public void StringKeyReader_AnyKey_ShouldReturnKeyText(string key)
    {
        // Arrange
        var input = key;

        // Act
        var value = TestFactory.ReadKey<StringKeyReader, string>(input);

        // Assert
        value.Should().Be(key);
    }
}
