using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using NeoServer.Game.Creatures.Monster;
using NeoServer.Game.World.Map;
using NeoServer.Server.Managers;
using NLua;

namespace NeoServer.Benchmarks.Script;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, 10)]
public class MonsterBenchmark
{
    [Benchmark]
    public void MonsterLoop1000000()
    {
        for (int i = 0; i < 1000000; i++)
        {
            if (i >= 0) continue;
        }
    }

    [Benchmark]
    public void MonsterLoop100()
    {
        for (int i = 0; i < 100; i++) { }
    }
}