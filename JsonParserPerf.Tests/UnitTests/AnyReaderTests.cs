using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class AnyReaderTests
{
    [Fact]
    public void Read_TypeUrlFirst_ShouldPackMessage()
    {
        // Arrange
        var json = $$"""{"@type":"{{TestFactory.TypeUrlPrefix}}LeafMessage","leafStringField":"leaf"}""";

        // Act
        var any = TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        any.TypeUrl.Should().Be(TestFactory.TypeUrlPrefix + "LeafMessage");
        any.Unpack<LeafMessage>().LeafStringField.Should().Be("leaf");
    }

    [Fact]
    public void Read_TypeUrlAfterFields_ShouldPackMessage()
    {
        // Arrange
        var json = $$"""{"leafInt32Field":7,"@type":"{{TestFactory.TypeUrlPrefix}}LeafMessage"}""";

        // Act
        var any = TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        any.Unpack<LeafMessage>().LeafInt32Field.Should().Be(7);
    }

    [Fact]
    public void Read_WellKnownPayload_ShouldPackValueMember()
    {
        // Arrange
        const string json = """{"@type":"type.googleapis.com/google.protobuf.Duration","value":"2s"}""";

        // Act
        var any = TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        any.Unpack<Duration>().Seconds.Should().Be(2);
    }

    [Fact]
    public void Read_WellKnownPayloadWithValueBeforeType_ShouldPackValueMember()
    {
        // Arrange
        const string json = """{"value":7,"@type":"type.googleapis.com/google.protobuf.Int32Value"}""";

        // Act
        var any = TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        any.Unpack<Int32Value>().Value.Should().Be(7);
    }

    [Fact]
    public void Read_MissingTypeUrl_ShouldThrowJsonException()
    {
        // Arrange
        const string json = """{"leafStringField":"leaf"}""";

        // Act
        var act = () => TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*@type*");
    }

    [Fact]
    public void Read_TypeNotInRegistry_ShouldThrowJsonException()
    {
        // Arrange
        var json = $$"""{"@type":"{{TestFactory.TypeUrlPrefix}}MissingMessage"}""";

        // Act
        var act = () => TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*type registry*");
    }

    [Fact]
    public void Read_WellKnownPayloadWithoutValue_ShouldThrowJsonException()
    {
        // Arrange
        const string json = """{"@type":"type.googleapis.com/google.protobuf.Duration"}""";

        // Act
        var act = () => TestFactory.ReadValue<AnyReader, Any>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*value member*");
    }
}
