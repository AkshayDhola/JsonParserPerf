using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;

namespace JsonParserPerf.Tests.IntegrationTests;

public class TimestampParityTests
{
    private sealed class TimestampHolder
    {
        [JsonPropertyName("createdAt")]
        public Timestamp? CreatedAt { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }

    [Theory]
    [MemberData(nameof(TestFactory.TimestampValues), MemberType = typeof(TestFactory))]
    public void Deserialize_ValidTimestamp_ShouldMatchGoogleParser(string value)
    {
        // Arrange
        var json = TestFactory.Quote(value);
        var expected = TestFactory.GoogleParser.Parse<Timestamp>(json);

        // Act
        var fromString = TestFactory.Deserialize<Timestamp>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<Timestamp>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidTimestampValues), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidTimestamp_ShouldBeRejectedByBothParsers(string value)
    {
        // Arrange
        var json = TestFactory.Quote(value);
        var google = () => TestFactory.GoogleParser.Parse<Timestamp>(json);

        // Act
        var ours = () => TestFactory.Deserialize<Timestamp>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Fact]
    public void Deserialize_TimestampPropertyOfPlainClass_ShouldParseTimestampAndSiblings()
    {
        // Arrange
        const string json = """{"createdAt":"2026-09-12T10:00:00Z","label":"deploy"}""";
        var expected = TestFactory.GoogleParser.Parse<Timestamp>("\"2026-09-12T10:00:00Z\"");

        // Act
        var holder = TestFactory.Deserialize<TimestampHolder>(json)!;

        // Assert
        holder.Label.Should().Be("deploy");
        holder.CreatedAt.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_NullTimestamp_ShouldReturnNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var timestamp = TestFactory.Deserialize<Timestamp>(json);

        // Assert
        timestamp.Should().BeNull();
    }
}
