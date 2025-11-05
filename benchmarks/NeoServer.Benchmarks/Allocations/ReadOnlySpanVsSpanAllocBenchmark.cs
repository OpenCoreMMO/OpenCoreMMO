using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace NeoServer.Benchmarks.Allocations;

[SimpleJob(RunStrategy.ColdStart, 50)]
[MemoryDiagnoser]
public class ReadOnlySpanVsSpanAllocBenchmark
{
    [Benchmark]
    public byte UsingVar()
    {
        var array = new byte[] { 1, 2, 3, 4 };
        return array[0];
    }

    [Benchmark]
    public byte UsingReadOnlySpan()
    {
        ReadOnlySpan<byte> array = [1, 2, 3, 4];
        return array[0];
    }

    [Benchmark]
    public byte UsingSpan()
    {
        Span<byte> array = new byte[] { 1, 2, 3, 4 };
        return array[0];
    }

    [Benchmark]
    public byte UsingSpanWithStackalloc()
    {
        Span<byte> array = [1, 2, 3, 4];
        return array[0];
    }
}