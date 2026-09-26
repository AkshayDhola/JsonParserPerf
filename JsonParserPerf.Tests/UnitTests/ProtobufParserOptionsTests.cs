using JsonParserPerf.Options;

namespace JsonParserPerf.Tests.UnitTests;

public class ProtobufParserOptionsTests
{
    [Fact]
    public void Default_NoArguments_ShouldUseDefaultLimitsWithoutInterning()
    {
        // Arrange
        var options = ProtobufParserOptions.Default;

        // Act
        var recursionLimit = options.RecursionLimit;

        // Assert
        recursionLimit.Should().Be(100);
        options.InternValues.Should().BeFalse();
        options.StringCache.Should().BeNull();
        options.BytesCache.Should().BeNull();
    }

    [Fact]
    public void Constructor_ExplicitRegistry_ShouldExposeSameRegistry()
    {
        // Arrange
        var registry = TestFactory.Registry;

        // Act
        var options = new ProtobufParserOptions(registry);

        // Assert
        options.TypeRegistry.Should().BeSameAs(registry);
    }

    [Fact]
    public void Constructor_InternValuesEnabled_ShouldCreateCaches()
    {
        // Arrange
        var registry = TestFactory.Registry;

        // Act
        var options = new ProtobufParserOptions(registry, internValues: true);

        // Assert
        options.InternValues.Should().BeTrue();
        options.StringCache.Should().NotBeNull();
        options.BytesCache.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullRegistry_ShouldThrowArgumentNullException()
    {
        // Arrange
        Google.Protobuf.Reflection.TypeRegistry registry = null!;

        // Act
        var act = () => new ProtobufParserOptions(registry);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RecursionLimitBelowOne_ShouldThrowArgumentOutOfRangeException(int limit)
    {
        // Arrange
        var registry = TestFactory.Registry;

        // Act
        var act = () => new ProtobufParserOptions(registry, recursionLimit: limit);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
