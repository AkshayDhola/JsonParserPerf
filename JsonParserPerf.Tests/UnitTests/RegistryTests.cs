using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JsonParserPerf.Protobuf;
using JsonParserPerf.Protobuf.Deserialization.Values;
using JsonParserPerf.Protobuf.Deserialization.WellKnown;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.UnitTests;

public class RegistryTests
{
    [Fact]
    public void FromLoadedAssemblies_GeneratedMessageLoaded_ShouldFindItsDescriptor()
    {
        // Arrange
        var descriptor = LeafMessage.Descriptor;

        // Act
        var registry = ProtobufTypeRegistry.FromLoadedAssemblies();

        // Assert
        registry.Find(descriptor.FullName).Should().BeSameAs(descriptor);
    }

    [Fact]
    public void FromLoadedAssemblies_CalledTwice_ShouldReturnCachedRegistry()
    {
        // Arrange
        var first = ProtobufTypeRegistry.FromLoadedAssemblies();

        // Act
        var second = ProtobufTypeRegistry.FromLoadedAssemblies();

        // Assert
        second.Should().BeSameAs(first);
    }

    [Theory]
    [InlineData(typeof(int), typeof(Int32Reader))]
    [InlineData(typeof(long), typeof(Int64Reader))]
    [InlineData(typeof(string), typeof(Utf8StringReader))]
    [InlineData(typeof(ByteString), typeof(BytesReader))]
    [InlineData(typeof(TopLevelEnum), typeof(EnumReader<TopLevelEnum>))]
    [InlineData(typeof(int?), typeof(NullableReader<int, Int32Reader>))]
    [InlineData(typeof(Timestamp), typeof(TimestampReader))]
    [InlineData(typeof(LeafMessage), typeof(MessageValueReader<LeafMessage>))]
    public void ReaderTypeOf_SupportedClrType_ShouldReturnMatchingReader(System.Type type, System.Type expected)
    {
        // Arrange
        var field = ScalarTypes.Descriptor.Fields[ScalarTypes.Int32FieldFieldNumber];

        // Act
        var reader = ValueReaderRegistry.ReaderTypeOf(type, field);

        // Assert
        reader.Should().Be(expected);
    }

    [Fact]
    public void ReaderTypeOf_UnsupportedClrType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var field = ScalarTypes.Descriptor.Fields[ScalarTypes.Int32FieldFieldNumber];

        // Act
        var act = () => ValueReaderRegistry.ReaderTypeOf(typeof(DateTime), field);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData(typeof(string), typeof(StringKeyReader))]
    [InlineData(typeof(bool), typeof(BoolKeyReader))]
    [InlineData(typeof(uint), typeof(UInt32Reader))]
    public void MapKeyReaderTypeOf_LegalKeyType_ShouldReturnKeyReader(System.Type type, System.Type expected)
    {
        // Arrange
        var keyType = type;

        // Act
        var reader = ValueReaderRegistry.MapKeyReaderTypeOf(keyType);

        // Assert
        reader.Should().Be(expected);
    }

    [Fact]
    public void MapKeyReaderTypeOf_DoubleKey_ShouldThrowNotSupportedException()
    {
        // Arrange
        var keyType = typeof(double);

        // Act
        var act = () => ValueReaderRegistry.MapKeyReaderTypeOf(keyType);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void WellKnownReaderTypeOf_WrapperType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var type = typeof(Int32Value);

        // Act
        var act = () => WellKnownReaderRegistry.ReaderTypeOf(type);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void WellKnownReaderTypeOf_OrdinaryMessage_ShouldReturnNull()
    {
        // Arrange
        var type = typeof(LeafMessage);

        // Act
        var reader = WellKnownReaderRegistry.ReaderTypeOf(type);

        // Assert
        reader.Should().BeNull();
    }

    [Fact]
    public void WellKnownReaderOf_WrapperType_ShouldReturnTopLevelReader()
    {
        // Arrange
        var expected = typeof(ReadValueFunc<Int32Value>);

        // Act
        var reader = WellKnownReaderRegistry.ReaderOf<Int32Value>();

        // Assert
        reader.Should().NotBeNull().And.BeOfType(expected);
    }
}
