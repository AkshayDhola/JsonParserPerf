using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Google.Protobuf;

namespace JsonParserPerf.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .AddJob(Job.Default.WithRuntime(CoreRuntime.Core80))
            .AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0));

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}

internal static class Guard
{
    public static void Equal(IMessage expected, IMessage actual, string source)
    {
        if (!expected.Equals(actual))
        {
            throw new InvalidOperationException($"{source} did not reproduce the source message.");
        }
    }
}
