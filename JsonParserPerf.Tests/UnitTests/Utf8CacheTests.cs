using System.Text;
using JsonParserPerf.Protobuf.Deserialization;

namespace JsonParserPerf.Tests.UnitTests;

public class Utf8CacheTests
{
    [Fact]
    public void TryGet_EmptyCache_ShouldReturnFalse()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes("key");

        // Act
        var found = cache.TryGet(key, out _, out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void TryGet_KeyAddedOnce_ShouldReturnFalse()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes("key");
        cache.TryGet(key, out _, out var hash);
        cache.Add(key, hash, "value");

        // Act
        var found = cache.TryGet(key, out _, out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void TryGet_KeyAddedTwice_ShouldReturnCachedValue()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes("key");
        cache.TryGet(key, out _, out var hash);
        cache.Add(key, hash, "first");
        cache.Add(key, hash, "second");

        // Act
        var found = cache.TryGet(key, out var value, out _);

        // Assert
        found.Should().BeTrue();
        value.Should().Be("second");
    }

    [Fact]
    public void TryGet_DifferentKey_ShouldReturnFalse()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes("key");
        cache.TryGet(key, out _, out var hash);
        cache.Add(key, hash, "value");
        cache.Add(key, hash, "value");

        // Act
        var found = cache.TryGet(Encoding.UTF8.GetBytes("other"), out _, out _);

        // Assert
        found.Should().BeFalse();
    }

    [Fact]
    public void TryGet_KeyLongerThan128Bytes_ShouldNeverCache()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes(new string('a', 129));
        cache.TryGet(key, out _, out var hash);
        cache.Add(key, hash, "value");
        cache.Add(key, hash, "value");

        // Act
        var found = cache.TryGet(key, out _, out var lookupHash);

        // Assert
        found.Should().BeFalse();
        lookupHash.Should().Be(0UL);
    }

    [Fact]
    public void TryGet_SameKeyTwice_ShouldReturnSameHash()
    {
        // Arrange
        var cache = new Utf8Cache<string>();
        var key = Encoding.UTF8.GetBytes("a longer key over eight bytes");

        // Act
        cache.TryGet(key, out _, out var first);
        cache.TryGet(key, out _, out var second);

        // Assert
        first.Should().Be(second);
    }
}
