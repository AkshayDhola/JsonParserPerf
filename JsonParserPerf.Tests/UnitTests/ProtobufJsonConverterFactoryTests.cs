using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Json;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class ProtobufJsonConverterFactoryTests
{
    [Theory]
    [InlineData(typeof(LeafMessage))]
    [InlineData(typeof(RecursiveMessage))]
    [InlineData(typeof(Timestamp))]
    [InlineData(typeof(Value))]
    public void CanConvert_GeneratedMessageType_ShouldReturnTrue(System.Type type)
    {
        // Arrange
        var factory = new ProtobufJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(type);

        // Assert
        canConvert.Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(IMessage))]
    [InlineData(typeof(object))]
    public void CanConvert_NonMessageType_ShouldReturnFalse(System.Type type)
    {
        // Arrange
        var factory = new ProtobufJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(type);

        // Assert
        canConvert.Should().BeFalse();
    }

    [Fact]
    public void CreateConverter_MessageType_ShouldReturnTypedConverter()
    {
        // Arrange
        var factory = new ProtobufJsonConverterFactory();

        // Act
        var converter = factory.CreateConverter(typeof(LeafMessage), new JsonSerializerOptions());

        // Assert
        converter.Should().BeOfType<ProtobufJsonConverter<LeafMessage>>();
    }

    [Fact]
    public void CreateConverter_SameTypeTwice_ShouldReturnCachedConverter()
    {
        // Arrange
        var factory = new ProtobufJsonConverterFactory();
        var first = factory.CreateConverter(typeof(LeafMessage), new JsonSerializerOptions());

        // Act
        var second = factory.CreateConverter(typeof(LeafMessage), new JsonSerializerOptions());

        // Assert
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        Options.ProtobufParserOptions options = null!;

        // Act
        var act = () => new ProtobufJsonConverterFactory(options);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
