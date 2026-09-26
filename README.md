# JsonParserPerf

A fast protobuf JSON parser for `System.Text.Json`. It reads the same JSON that Google's `JsonParser` reads, but runs about 2x faster and allocates less memory.

## Usage

```csharp
var options = new JsonSerializerOptions().AddProtobuf(registry); // registry is optional
var message = JsonSerializer.Deserialize<MyMessage>(json, options);
```

## Projects

- `JsonParserPerf`: the parser library (targets net8.0 and net10.0)
- `JsonParserPerf.ProtobufDataModels`: `.proto` test models
- `JsonParserPerf.Tests`: unit tests and parity tests against Google's parser
- `JsonParserPerf.Benchmarks`: BenchmarkDotNet benchmarks

## Run

```bash
dotnet test
dotnet run -c Release --project JsonParserPerf.Benchmarks -f net10.0 -- --filter '*'
```

## Benchmarks

Intel Core i7-8665U, Arch Linux, `performance` power profile, BenchmarkDotNet 0.15.8.

**500 customers: our parser vs Google `JsonParser`**

| Parser             | Runtime | Mean     | Ratio | Allocated |
|--------------------|---------|---------:|------:|----------:|
| Google JsonParser  | .NET 10 | 947.5 μs |  1.00 |    781 KB |
| **Ours**           | .NET 10 | 434.1 μs |  0.46 |    206 KB |
| Google JsonParser  | .NET 8  | 1,099 μs |  1.00 |    781 KB |
| **Ours**           | .NET 8  | 496.0 μs |  0.45 |    206 KB |

**Large model deserialization (200 items): JSON vs binary protobuf**

| Parser             | Runtime | Mean      | Ratio | Allocated |
|--------------------|---------|----------:|------:|----------:|
| Binary `ParseFrom` | .NET 10 |  1.71 ms  |  1.00 |   2.43 MB |
| **Ours (JSON)**    | .NET 10 |  3.68 ms  |  2.16 |   2.46 MB |
| Google (JSON)      | .NET 10 |  9.35 ms  |  5.49 |   6.22 MB |
| Binary `ParseFrom` | .NET 8  |  2.03 ms  |  1.00 |   2.43 MB |
| **Ours (JSON)**    | .NET 8  |  4.50 ms  |  2.21 |   3.02 MB |
| Google (JSON)      | .NET 8  | 10.63 ms  |  5.23 |   6.24 MB |

**Summary**

- Our parser is 2.2–2.5x faster than Google `JsonParser` and allocates 52–74% less memory.
- On .NET 10, our JSON parser allocates about the same memory as binary protobuf.
- .NET 10 is about 15% faster than .NET 8 across all parsers.
