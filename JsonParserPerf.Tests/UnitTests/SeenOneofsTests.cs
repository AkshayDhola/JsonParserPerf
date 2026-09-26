using JsonParserPerf.Protobuf.Deserialization;

namespace JsonParserPerf.Tests.UnitTests;

public class SeenOneofsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(200)]
    public void TryAdd_NewIndex_ShouldReturnTrue(int index)
    {
        // Arrange
        var seen = new SeenOneofs();

        // Act
        var added = seen.TryAdd(index);

        // Assert
        added.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(200)]
    public void TryAdd_IndexAlreadyAdded_ShouldReturnFalse(int index)
    {
        // Arrange
        var seen = new SeenOneofs();
        seen.TryAdd(index);

        // Act
        var added = seen.TryAdd(index);

        // Assert
        added.Should().BeFalse();
    }

    [Fact]
    public void TryAdd_IndexesBelowAndAbove64_ShouldTrackEachSeparately()
    {
        // Arrange
        var seen = new SeenOneofs();
        seen.TryAdd(3);
        seen.TryAdd(70);

        // Act
        var addedLow = seen.TryAdd(6);
        var addedHigh = seen.TryAdd(65);

        // Assert
        addedLow.Should().BeTrue();
        addedHigh.Should().BeTrue();
    }
}
