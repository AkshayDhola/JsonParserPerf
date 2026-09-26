using System.Text;
using System.Text.Json;

namespace JsonParserPerf.Tests.UnitTests;

public class JsonTokenTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("\"42\"", 42)]
    [InlineData("1.0", 1)]
    [InlineData("1e3", 1000)]
    [InlineData("-7", -7)]
    public void TryReadNumber_ValidInt32Token_ShouldReturnTrueWithValue(string json, int expected)
    {
        // Arrange
        var input = json;

        // Act
        var result = TestFactory.TryReadNumber<int>(input, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("\"nine\"")]
    [InlineData("2147483648")]
    [InlineData("true")]
    [InlineData("null")]
    public void TryReadNumber_InvalidInt32Token_ShouldReturnFalse(string json)
    {
        // Arrange
        var input = json;

        // Act
        var result = TestFactory.TryReadNumber<int>(input, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("\"NaN\"")]
    [InlineData("\"Infinity\"")]
    [InlineData("\"-Infinity\"")]
    public void TryReadDouble_NonFiniteLiteralAsString_ShouldReturnTrue(string json)
    {
        // Arrange
        var input = json;

        // Act
        var result = TestFactory.TryReadDouble(input, out var value);

        // Assert
        result.Should().BeTrue();
        double.IsFinite(value).Should().BeFalse();
    }

    [Theory]
    [InlineData("1e400")]
    [InlineData("\"nan\"")]
    [InlineData("\"infinity\"")]
    [InlineData("\"1e400\"")]
    public void TryReadDouble_NonFiniteNotSpelledCanonically_ShouldReturnFalse(string json)
    {
        // Arrange
        var input = json;

        // Act
        var result = TestFactory.TryReadDouble(input, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryReadDouble_FiniteNumber_ShouldReturnTrueWithValue()
    {
        // Arrange
        const string json = "1.5";

        // Act
        var result = TestFactory.TryReadDouble(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(1.5);
    }

    [Fact]
    public void UnescapedUtf8_PlainString_ShouldReturnRawBytes()
    {
        // Arrange
        const string json = "\"abc\"";

        // Act
        var bytes = TestFactory.UnescapedUtf8(json);

        // Assert
        bytes.Should().Equal(Encoding.UTF8.GetBytes("abc"));
    }

    [Fact]
    public void UnescapedUtf8_EscapedString_ShouldReturnUnescapedBytes()
    {
        // Arrange
        const string json = "\"a\\u0042c\"";

        // Act
        var bytes = TestFactory.UnescapedUtf8(json);

        // Assert
        bytes.Should().Equal(Encoding.UTF8.GetBytes("aBc"));
    }

    [Fact]
    public void CheckDepth_DepthWithinLimit_ShouldNotThrow()
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: 3);

        // Act
        var act = () => TestFactory.CheckDepthAt(3, options);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CheckDepth_DepthAboveLimit_ShouldThrowJsonException()
    {
        // Arrange
        var options = TestFactory.ParserOptions(recursionLimit: 3);

        // Act
        var act = () => TestFactory.CheckDepthAt(4, options);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*recursion limit of 3*");
    }
}
