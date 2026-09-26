using System.Text.Json;
using System.Text.Json.Serialization;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class MessageTypesParityTests
{
    private sealed class MessageHolder
    {
        [JsonPropertyName("leaf")]
        public LeafMessage? Leaf { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    [Theory]
    [MemberData(nameof(TestFactory.MessagePayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_MessagePayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<MessageTypes>(json);

        // Act
        var fromString = TestFactory.Deserialize<MessageTypes>(json);
        var fromUtf8 = TestFactory.DeserializeUtf8<MessageTypes>(json);

        // Assert
        fromString.Should().Be(expected);
        fromUtf8.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestFactory.InvalidMessagePayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_InvalidMessagePayload_ShouldBeRejectedByBothParsers(string json)
    {
        // Arrange
        var google = () => TestFactory.GoogleParser.Parse<MessageTypes>(json);

        // Act
        var ours = () => TestFactory.Deserialize<MessageTypes>(json);

        // Assert
        ours.Should().Throw<JsonException>();
        google.Should().Throw<Exception>();
    }

    [Theory]
    [MemberData(nameof(TestFactory.RecursivePayloads), MemberType = typeof(TestFactory))]
    public void Deserialize_RecursivePayload_ShouldMatchGoogleParser(string json)
    {
        // Arrange
        var expected = TestFactory.GoogleParser.Parse<RecursiveMessage>(json);

        // Act
        var message = TestFactory.Deserialize<RecursiveMessage>(json);

        // Assert
        message.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_FiftyLevelsOfNesting_ShouldMatchGoogleParser()
    {
        // Arrange
        var json = TestFactory.NestedRecursiveJson(50);
        var expected = TestFactory.GoogleParser.Parse<RecursiveMessage>(json);

        // Act
        var message = TestFactory.Deserialize<RecursiveMessage>(json);

        // Assert
        message.Should().Be(expected);
    }

    [Fact]
    public void Deserialize_MessagePropertyOfPlainClass_ShouldParseMessageAndSiblings()
    {
        // Arrange
        const string json = """{"leaf":{"leafStringField":"text"},"count":2}""";

        // Act
        var holder = TestFactory.Deserialize<MessageHolder>(json)!;

        // Assert
        holder.Leaf.Should().Be(new LeafMessage { LeafStringField = "text" });
        holder.Count.Should().Be(2);
    }

    [Fact]
    public void Deserialize_ListOfMessages_ShouldParseEveryElement()
    {
        // Arrange
        const string json = """[{"leafInt32Field":1},{"leafStringField":"x"}]""";

        // Act
        var list = TestFactory.Deserialize<List<LeafMessage>>(json)!;

        // Assert
        list.Should().Equal(new LeafMessage { LeafInt32Field = 1 }, new LeafMessage { LeafStringField = "x" });
    }

    [Fact]
    public void Deserialize_DictionaryOfMessages_ShouldParseEveryValue()
    {
        // Arrange
        const string json = """{"a":{"leafInt32Field":2}}""";

        // Act
        var map = TestFactory.Deserialize<Dictionary<string, LeafMessage>>(json)!;

        // Assert
        map.Should().ContainKey("a").WhoseValue.Should().Be(new LeafMessage { LeafInt32Field = 2 });
    }

    [Fact]
    public void Deserialize_NestingAboveSerializerMaxDepth_ShouldThrowJsonException()
    {
        // Arrange
        var options = TestFactory.SerializerOptions();
        options.MaxDepth = 5;
        var json = TestFactory.NestedRecursiveJson(6);

        // Act
        var act = () => TestFactory.Deserialize<RecursiveMessage>(json, options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_NestingAtSerializerMaxDepth_ShouldSucceed()
    {
        // Arrange
        var options = TestFactory.SerializerOptions();
        options.MaxDepth = 5;
        var json = TestFactory.NestedRecursiveJson(5);

        // Act
        var message = TestFactory.Deserialize<RecursiveMessage>(json, options);

        // Assert
        message.Should().NotBeNull();
    }
}
