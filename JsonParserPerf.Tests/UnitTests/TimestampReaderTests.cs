using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;

namespace JsonParserPerf.Tests.UnitTests;

public class TimestampReaderTests
{
    [Theory]
    [InlineData("2026-09-12T10:00:00.1Z", 100_000_000)]
    [InlineData("2026-09-12T10:00:00.12Z", 120_000_000)]
    [InlineData("2026-09-12T10:00:00.123Z", 123_000_000)]
    [InlineData("2026-09-12T10:00:00.123456Z", 123_456_000)]
    [InlineData("2026-09-12T10:00:00.123456789Z", 123_456_789)]
    [InlineData("2026-09-12T10:00:00.000000001Z", 1)]
    [InlineData("2026-09-12T10:00:00Z", 0)]
    public void Read_FractionalSeconds_ShouldReturnNanos(string value, int expectedNanos)
    {
        // Arrange
        var json = TestFactory.Quote(value);

        // Act
        var timestamp = TestFactory.ReadValue<TimestampReader, Timestamp>(json);

        // Assert
        timestamp.Nanos.Should().Be(expectedNanos);
    }

    [Theory]
    [InlineData("1969-07-20T20:17:40Z", -14182940L)]
    [InlineData("1970-01-01T00:00:00Z", 0L)]
    [InlineData("0001-01-01T00:00:00Z", -62135596800L)]
    [InlineData("9999-12-31T23:59:59Z", 253402300799L)]
    [InlineData("2024-02-29T12:00:00Z", 1709208000L)]
    [InlineData("1900-03-01T00:00:00Z", -2203891200L)]
    public void Read_BoundaryOrPreEpochInstant_ShouldReturnUnixSeconds(string value, long expectedSeconds)
    {
        // Arrange
        var json = TestFactory.Quote(value);

        // Act
        var timestamp = TestFactory.ReadValue<TimestampReader, Timestamp>(json);

        // Assert
        timestamp.Seconds.Should().Be(expectedSeconds);
    }

    [Theory]
    [InlineData("2026-09-12T15:30:00+05:30")]
    [InlineData("2026-09-12T04:30:00-05:30")]
    public void Read_UtcOffset_ShouldNormalizeToUtc(string value)
    {
        // Arrange
        var expected = TestFactory.ReadValue<TimestampReader, Timestamp>(TestFactory.Quote("2026-09-12T10:00:00Z"));

        // Act
        var timestamp = TestFactory.ReadValue<TimestampReader, Timestamp>(TestFactory.Quote(value));

        // Assert
        timestamp.Should().Be(expected);
    }

    [Fact]
    public void Read_EscapedString_ShouldReturnSameInstant()
    {
        // Arrange
        var expected = TestFactory.ReadValue<TimestampReader, Timestamp>(TestFactory.Quote("2026-09-12T10:00:00Z"));

        // Act
        var timestamp = TestFactory.ReadValue<TimestampReader, Timestamp>("\"2026-09-12T10:00:00\\u005A\"");

        // Assert
        timestamp.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidTimestampValues), MemberType = typeof(TestFactory))]
    public void Read_InvalidTimestamp_ShouldThrowJsonException(string value)
    {
        // Arrange
        var json = TestFactory.Quote(value);

        // Act
        var act = () => TestFactory.ReadValue<TimestampReader, Timestamp>(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_NumberToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<TimestampReader, Timestamp>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected a string*");
    }
}
