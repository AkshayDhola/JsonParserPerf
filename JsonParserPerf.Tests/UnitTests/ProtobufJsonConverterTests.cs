using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class ProtobufJsonConverterTests
{
    [Fact]
    public void Read_NullToken_ShouldReturnNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var message = TestFactory.Deserialize<LeafMessage>(json);

        // Assert
        message.Should().BeNull();
    }

    [Fact]
    public void Read_ObjectToken_ShouldParseMessage()
    {
        // Arrange
        const string json = """{"leafStringField":"text"}""";

        // Act
        var message = TestFactory.Deserialize<LeafMessage>(json);

        // Assert
        message.Should().Be(new LeafMessage { LeafStringField = "text" });
    }

    [Fact]
    public void Read_WellKnownTypeAtTopLevel_ShouldUseItsJsonForm()
    {
        // Arrange
        const string json = "\"5s\"";

        // Act
        var duration = TestFactory.Deserialize<Duration>(json);

        // Assert
        duration!.Seconds.Should().Be(5);
    }

    [Fact]
    public void Write_AnyMessage_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new ProtobufJsonConverter<LeafMessage>();
        using var writer = new Utf8JsonWriter(new MemoryStream());

        // Act
        var act = () => converter.Write(writer, new LeafMessage(), new JsonSerializerOptions());

        // Assert
        act.Should().Throw<NotSupportedException>().WithMessage("*JsonFormatter*");
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        Options.ProtobufParserOptions options = null!;

        // Act
        var act = () => new ProtobufJsonConverter<LeafMessage>(options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HandleNull_Always_ShouldBeTrue()
    {
        // Arrange
        var converter = new ProtobufJsonConverter<LeafMessage>();

        // Act
        var handleNull = converter.HandleNull;

        // Assert
        handleNull.Should().BeTrue();
    }
}
