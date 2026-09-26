using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Benchmarks;

[MemoryDiagnoser]
public class LargeModelDeserializationBenchmarks
{
    private byte[] _binary = null!;
    private string _json = null!;

    [GlobalSetup]
    public void Setup()
    {
        var source = BenchmarkData.Large(items: 200);
        _binary = source.ToByteArray();
        _json = BenchmarkData.Formatter.Format(source);

        Guard.Equal(source, Binary_ParseFrom(), nameof(Binary_ParseFrom));
        Guard.Equal(source, Json_OurParser(), nameof(Json_OurParser));
        Guard.Equal(source, Json_GoogleParser(), nameof(Json_GoogleParser));
    }

    [Benchmark(Baseline = true)]
    public LargePayload Binary_ParseFrom()
    {
        return LargePayload.Parser.ParseFrom(_binary);
    }

    [Benchmark]
    public LargePayload Json_OurParser()
    {
        return JsonSerializer.Deserialize<LargePayload>(_json, BenchmarkData.SerializerOptions)!;
    }

    [Benchmark]
    public LargePayload Json_GoogleParser()
    {
        return BenchmarkData.GoogleParser.Parse<LargePayload>(_json);
    }
}
