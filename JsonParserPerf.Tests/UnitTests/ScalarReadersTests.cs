using System.Text;
using System.Text.Json;
using Google.Protobuf;
using JsonParserPerf.Protobuf.Deserialization.Values;

namespace JsonParserPerf.Tests.UnitTests;

public class ScalarReadersTests
{
    [Theory]
    [InlineData("-2147483648", int.MinValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("\"5\"", 5)]
    [InlineData("1e3", 1000)]
    public void Int32Reader_ValidValue_ShouldReturnInt32(string json, int expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<Int32Reader, int>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    [InlineData("1.5")]
    [InlineData("\"x\"")]
    [InlineData("true")]
    public void Int32Reader_InvalidValue_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<Int32Reader, int>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*int32*");
    }

    [Fact]
    public void Int32Reader_PropertyNameToken_ShouldParseMapKey()
    {
        // Arrange
        const string key = "-12";

        // Act
        var value = TestFactory.ReadKey<Int32Reader, int>(key);

        // Assert
        value.Should().Be(-12);
    }

    [Theory]
    [InlineData("\"-9223372036854775808\"", long.MinValue)]
    [InlineData("\"9223372036854775807\"", long.MaxValue)]
    [InlineData("123", 123L)]
    public void Int64Reader_ValidValue_ShouldReturnInt64(string json, long expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<Int64Reader, long>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void Int64Reader_NonNumericString_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "\"nine\"";

        // Act
        var act = () => TestFactory.ReadValue<Int64Reader, long>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*int64*");
    }

    [Theory]
    [InlineData("4294967296")]
    [InlineData("-1")]
    public void UInt32Reader_OutOfRange_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<UInt32Reader, uint>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*uint32*");
    }

    [Fact]
    public void UInt64Reader_MaxValueAsString_ShouldReturnUInt64()
    {
        // Arrange
        const string json = "\"18446744073709551615\"";

        // Act
        var value = TestFactory.ReadValue<UInt64Reader, ulong>(json);

        // Assert
        value.Should().Be(ulong.MaxValue);
    }

    [Fact]
    public void UInt64Reader_Negative_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "-1";

        // Act
        var act = () => TestFactory.ReadValue<UInt64Reader, ulong>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*uint64*");
    }

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("\"1.5\"", 1.5)]
    [InlineData("\"-Infinity\"", double.NegativeInfinity)]
    public void DoubleReader_ValidValue_ShouldReturnDouble(string json, double expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<DoubleReader, double>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void DoubleReader_NaNString_ShouldReturnNaN()
    {
        // Arrange
        const string json = "\"NaN\"";

        // Act
        var value = TestFactory.ReadValue<DoubleReader, double>(json);

        // Assert
        double.IsNaN(value).Should().BeTrue();
    }

    [Theory]
    [InlineData("\"1.5x\"")]
    [InlineData("\"nan\"")]
    [InlineData("\"1e400\"")]
    [InlineData("true")]
    public void DoubleReader_InvalidValue_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<DoubleReader, double>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*double*");
    }

    [Fact]
    public void FloatReader_FiniteValueAboveFloatRange_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "1e39";

        // Act
        var act = () => TestFactory.ReadValue<FloatReader, float>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*float*");
    }

    [Fact]
    public void FloatReader_InfinityString_ShouldReturnPositiveInfinity()
    {
        // Arrange
        const string json = "\"Infinity\"";

        // Act
        var value = TestFactory.ReadValue<FloatReader, float>(json);

        // Assert
        value.Should().Be(float.PositiveInfinity);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BoolReader_BooleanLiteral_ShouldReturnBool(string json, bool expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<BoolReader, bool>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"true\"")]
    [InlineData("1")]
    public void BoolReader_NonBooleanToken_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<BoolReader, bool>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*bool*");
    }

    [Theory]
    [InlineData("\"ascii\"", "ascii")]
    [InlineData("\"café\"", "café")]
    [InlineData("\"a\\u0042c\"", "aBc")]
    [InlineData("\"\"", "")]
    public void Utf8StringReader_StringToken_ShouldReturnDecodedText(string json, string expected)
    {
        // Arrange
        var input = json;

        // Act
        var value = TestFactory.ReadValue<Utf8StringReader, string>(input);

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void Utf8StringReader_NumberToken_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "5";

        // Act
        var act = () => TestFactory.ReadValue<Utf8StringReader, string>(json);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("Expected a string*");
    }

    [Fact]
    public void Utf8StringReader_RepeatedValueWithInterning_ShouldReturnCachedInstance()
    {
        // Arrange
        var options = TestFactory.ParserOptions(internValues: true);
        const string json = "\"repeated value\"";
        TestFactory.ReadValue<Utf8StringReader, string>(json, options);
        var second = TestFactory.ReadValue<Utf8StringReader, string>(json, options);

        // Act
        var third = TestFactory.ReadValue<Utf8StringReader, string>(json, options);

        // Assert
        third.Should().BeSameAs(second);
    }

    [Theory]
    [MemberData(nameof(TestFactory.Utf8StringValues), MemberType = typeof(TestFactory))]
    public void Utf8StringReader_InterningOnOrOff_ShouldReturnSameText(string expected)
    {
        // Arrange
        var json = TestFactory.Quote(expected);
        var interning = TestFactory.ParserOptions(internValues: true);
        var plain = TestFactory.ParserOptions();

        // Act
        var interned = TestFactory.ReadValue<Utf8StringReader, string>(json, interning);
        var decoded = TestFactory.ReadValue<Utf8StringReader, string>(json, plain);

        // Assert
        interned.Should().Be(expected);
        decoded.Should().Be(expected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(20)]
    public void Utf8StringReader_MalformedUtf8_ShouldThrowSameExceptionWithOrWithoutInterning(int asciiPrefix)
    {
        // Arrange
        var utf8 = Encoding.UTF8.GetBytes($"\"{new string('a', asciiPrefix)}X\"");
        utf8[^2] = 0xFF;
        var interning = TestFactory.ParserOptions(internValues: true);
        var plain = TestFactory.ParserOptions();

        // Act
        var interned = Record.Exception(() => TestFactory.ReadValue<Utf8StringReader, string>(utf8, interning));
        var decoded = Record.Exception(() => TestFactory.ReadValue<Utf8StringReader, string>(utf8, plain));

        // Assert
        decoded.Should().NotBeNull();
        interned.Should().NotBeNull().And.BeOfType(decoded!.GetType());
    }

    [Theory]
    [InlineData("\"aGk=\"", "hi")]
    [InlineData("\"\\/w==\"", "ÿ")]
    public void BytesReader_ValidBase64_ShouldReturnBytes(string json, string expectedLatin1)
    {
        // Arrange
        var expected = Encoding.Latin1.GetBytes(expectedLatin1);

        // Act
        var value = TestFactory.ReadValue<BytesReader, ByteString>(json);

        // Assert
        value.ToByteArray().Should().Equal(expected);
    }

    [Fact]
    public void BytesReader_EmptyString_ShouldReturnEmptyByteString()
    {
        // Arrange
        const string json = "\"\"";

        // Act
        var value = TestFactory.ReadValue<BytesReader, ByteString>(json);

        // Assert
        value.Should().BeSameAs(ByteString.Empty);
    }

    [Theory]
    [InlineData("\"not base64\"")]
    [InlineData("\"aGk\"")]
    public void BytesReader_InvalidBase64_ShouldThrowJsonException(string json)
    {
        // Arrange
        var input = json;

        // Act
        var act = () => TestFactory.ReadValue<BytesReader, ByteString>(input);

        // Assert
        act.Should().Throw<JsonException>().WithMessage("*base64*");
    }

    [Fact]
    public void BytesReader_RepeatedValueWithInterning_ShouldReturnCachedInstance()
    {
        // Arrange
        var options = TestFactory.ParserOptions(internValues: true);
        const string json = "\"aGVsbG8=\"";
        TestFactory.ReadValue<BytesReader, ByteString>(json, options);
        var second = TestFactory.ReadValue<BytesReader, ByteString>(json, options);

        // Act
        var third = TestFactory.ReadValue<BytesReader, ByteString>(json, options);

        // Assert
        third.Should().BeSameAs(second);
    }
}
