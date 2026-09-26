using System.Globalization;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Tests.IntegrationTests;

public class DoubleParsingParityTests
{
    [Theory]
    [MemberData(nameof(TestFactory.DoubleLiterals), MemberType = typeof(TestFactory))]
    public void Deserialize_DoubleLiteral_ShouldMatchGoogleParserBitForBit(string literal)
    {
        // Arrange
        var json = $$"""{"doubleField":{{literal}}}""";
        var expected = TestFactory.GoogleParser.Parse<ScalarTypes>(json).DoubleField;

        // Act
        var actual = TestFactory.DeserializeUtf8<ScalarTypes>(json)!.DoubleField;

        // Assert
        BitConverter.DoubleToInt64Bits(actual).Should().Be(BitConverter.DoubleToInt64Bits(expected));
    }

    [Fact]
    public void Deserialize_RandomRoundTripDoubles_ShouldMatchGoogleParserBitForBit()
    {
        // Arrange
        var random = new Random(20260923);
        var literals = new List<string>();
        while (literals.Count < 2000)
        {
            var value = BitConverter.Int64BitsToDouble(random.NextInt64());
            if (double.IsFinite(value))
            {
                literals.Add(value.ToString("R", CultureInfo.InvariantCulture));
            }
        }

        // Act
        var mismatches = literals
            .Select(literal => $$"""{"doubleField":{{literal}}}""")
            .Where(json => BitConverter.DoubleToInt64Bits(TestFactory.DeserializeUtf8<ScalarTypes>(json)!.DoubleField)
                           != BitConverter.DoubleToInt64Bits(TestFactory.GoogleParser.Parse<ScalarTypes>(json).DoubleField))
            .ToList();

        // Assert
        mismatches.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestFactory.FloatLiterals), MemberType = typeof(TestFactory))]
    public void Deserialize_FloatLiteral_ShouldMatchGoogleParserBitForBit(string literal)
    {
        // Arrange
        var json = $$"""{"floatField":{{literal}}}""";
        var expected = TestFactory.GoogleParser.Parse<ScalarTypes>(json).FloatField;

        // Act
        var actual = TestFactory.DeserializeUtf8<ScalarTypes>(json)!.FloatField;

        // Assert
        BitConverter.SingleToInt32Bits(actual).Should().Be(BitConverter.SingleToInt32Bits(expected));
    }
}
