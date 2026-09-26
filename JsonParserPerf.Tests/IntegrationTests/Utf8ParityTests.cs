using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class Utf8ParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.Utf8StringValues), MemberType = typeof(TestFactory))]
    public void Deserialize_StringValueWithInterningOnOrOff_ShouldMatchGoogleParser(string value)
    {
        // Arrange
        var json = $$"""{"stringField":{{TestFactory.Quote(value)}}}""";
        var expected = TestFactory.GoogleParser.Parse<ScalarTypes>(json);
        var interning = TestFactory.SerializerOptions(TestFactory.ParserOptions(internValues: true));
        var plain = TestFactory.SerializerOptions(TestFactory.ParserOptions());

        // Act
        var interned = TestFactory.DeserializeUtf8<ScalarTypes>(json, interning);
        var decoded = TestFactory.DeserializeUtf8<ScalarTypes>(json, plain);

        // Assert
        interned.Should().Be(expected);
        decoded.Should().Be(expected);
    }

    [Theory]
    [InlineData("kéy")]
    [InlineData("1234567890123456")]
    [InlineData("🙂")]
    public void Deserialize_NonAsciiMapKey_ShouldMatchGoogleParser(string key)
    {
        // Arrange
        var json = """{"stringKeyMap":{""" + TestFactory.Quote(key) + """:"v"}}""";
        var expected = TestFactory.GoogleParser.Parse<MapKeyTypes>(json);
        var interning = TestFactory.SerializerOptions(TestFactory.ParserOptions(internValues: true));

        // Act
        var interned = TestFactory.DeserializeUtf8<MapKeyTypes>(json, interning);
        var decoded = TestFactory.DeserializeUtf8<MapKeyTypes>(json);

        // Assert
        interned.Should().Be(expected);
        decoded.Should().Be(expected);
        decoded!.StringKeyMap.Should().ContainSingle().Which.Key.Should().Be(key);
    }
}
