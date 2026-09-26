using System.Text;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Tests.UnitTests;

public class DurationReaderTests
{
    [Theory]
    [InlineData("1.500s", 1L, 500_000_000)]
    [InlineData("0s", 0L, 0)]
    [InlineData("0.5s", 0L, 500_000_000)]
    [InlineData("-1.5s", -1L, -500_000_000)]
    [InlineData("-0.000000001s", 0L, -1)]
    [InlineData("1.000000001s", 1L, 1)]
    [InlineData("315576000000s", 315_576_000_000L, 0)]
    [InlineData("-315576000000s", -315_576_000_000L, 0)]
    public void Read_ValidDuration_ShouldReturnSecondsAndNanos(string value, long seconds, int nanos)
    {
        // Arrange
        var json = TestFactory.Quote(value);

        // Act
        var duration = TestFactory.ReadValue<DurationReader, Duration>(json);

        // Assert
        duration.Seconds.Should().Be(seconds);
        duration.Nanos.Should().Be(nanos);
    }

    [Theory]
    [InlineData("3.5")]
    [InlineData("1s1s")]
    [InlineData("+1s")]
    [InlineData(".5s")]
    [InlineData("5.s")]
    [InlineData("1.1234567890s")]
    [InlineData("315576000001s")]
    [InlineData("315576000000.1s")]
    [InlineData(" 1s")]
    [InlineData("1S")]
    [InlineData("1e3s")]
    [InlineData("--1s")]
    [InlineData("s")]
    public void Read_InvalidDuration_ShouldThrowJsonException(string value)
    {
        // Arrange
        var json = TestFactory.Quote(value);

        // Act
        var act = () => TestFactory.ReadValue<DurationReader, Duration>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*duration*");
    }

    [Fact]
    public void Read_NumberToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<DurationReader, Duration>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected a string*");
    }

    [Theory]
    [InlineData("5", 500_000_000)]
    [InlineData("123", 123_000_000)]
    [InlineData("000000001", 1)]
    [InlineData("999999999", 999_999_999)]
    public void TryParseNanos_OneToNineDigits_ShouldScaleToNanoseconds(string digits, int expected)
    {
        // Arrange
        var utf8 = Encoding.ASCII.GetBytes(digits);

        // Act
        var parsed = DurationReader.TryParseNanos(utf8, out var nanos);

        // Assert
        parsed.Should().BeTrue();
        nanos.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890")]
    [InlineData("12a")]
    [InlineData("-1")]
    public void TryParseNanos_InvalidDigits_ShouldReturnFalse(string digits)
    {
        // Arrange
        var utf8 = Encoding.ASCII.GetBytes(digits);

        // Act
        var parsed = DurationReader.TryParseNanos(utf8, out _);

        // Assert
        parsed.Should().BeFalse();
    }
}
