using System;
using BenchmarkDotNet.Running;
using NeoServer.Benchmarks.Collections;
using NeoServer.Benchmarks.EventAggregator;

namespace NeoServer.Benchmarks;

internal class Program
{
    private static void Main(string[] args)
    {
        //  BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugBuildConfig());
        //BenchmarkRunner.Run<StackVsListInsertZeroIndex>();
        BenchmarkRunner.Run<EventAggregatorBenchmark>();
        
        Console.ReadKey();
    }
}