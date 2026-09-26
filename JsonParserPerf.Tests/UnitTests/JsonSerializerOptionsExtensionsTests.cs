using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf;
using JsonParserPerf.Protobuf.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class JsonSerializerOptionsExtensionsTests
{
    [Fact]
    public void AddProtobuf_WithoutRegistry_ShouldAddConverterFactory()
    {
        // Arrange
        var options = new JsonSerializerOptions();

        // Act
        var result = options.AddProtobuf();

        // Assert
        result.Should().BeSameAs(options);
        options.Converters.Should().ContainSingle().Which.Should().BeOfType<ProtobufJsonConverterFactory>();
    }

    [Fact]
    public void AddProtobuf_WithRegistry_ShouldResolveAnyThroughThatRegistry()
    {
        // Arrange
        var options = new JsonSerializerOptions().AddProtobuf(TestFactory.Registry);
        var json = $$"""{"@type":"{{TestFactory.TypeUrlPrefix}}LeafMessage","leafInt32Field":3}""";

        // Act
        var any = JsonSerializer.Deserialize<Any>(json, options);

        // Assert
        any!.Unpack<LeafMessage>().LeafInt32Field.Should().Be(3);
    }

    [Fact]
    public void AddProtobuf_NullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        JsonSerializerOptions options = null!;

        // Act
        var act = () => options.AddProtobuf();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_WithoutAddProtobuf_ShouldThrow()
    {
        // Arrange
        const string json = "\"2026-09-12T10:00:00Z\"";

        // Act
        var act = () => JsonSerializer.Deserialize<Timestamp>(json);

        // Assert
        act.Should().Throw<Exception>();
    }
}
