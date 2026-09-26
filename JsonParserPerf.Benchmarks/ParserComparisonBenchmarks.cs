using System.Text.Json;
using BenchmarkDotNet.Attributes;
using JsonParserPerf.ProtobufDataModels;

namespace JsonParserPerf.Benchmarks;

[MemoryDiagnoser]
public class ParserComparisonBenchmarks
{
    private string _json = null!;

    [GlobalSetup]
    public void Setup()
    {
        var source = BenchmarkData.Customers(count: 500);
        _json = BenchmarkData.Formatter.Format(source);

        Guard.Equal(source, Google_JsonParser(), nameof(Google_JsonParser));
        Guard.Equal(source, Ours_JsonSerializer(), nameof(Ours_JsonSerializer));
    }

    [Benchmark(Baseline = true)]
    public CustomerList Google_JsonParser()
    {
        return BenchmarkData.GoogleParser.Parse<CustomerList>(_json);
    }

    [Benchmark]
    public CustomerList Ours_JsonSerializer()
    {
        return JsonSerializer.Deserialize<CustomerList>(_json, BenchmarkData.SerializerOptions)!;
    }
}
