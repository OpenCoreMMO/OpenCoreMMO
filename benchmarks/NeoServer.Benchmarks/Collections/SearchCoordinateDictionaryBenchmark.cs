using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using NeoServer.Domain.Common.Location.Structs.Helpers;

namespace NeoServer.Benchmarks.Collections;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, 1)]
public class SearchCoordinateDictionaryBenchmark
{
    private ConcurrentDictionary<CoordinateWithHashCode, string> data;
    private ConcurrentDictionary<CoordinateWithHashCodeAndEquals, string> data2;

    [GlobalSetup]
    public void Setup()
    {
        data = new ConcurrentDictionary<CoordinateWithHashCode, string>();
        data2 = new ConcurrentDictionary<CoordinateWithHashCodeAndEquals, string>();
        for (var x = 0; x < 2000; x++)
        for (var y = 0; y < 2000; y++)
        {
            data.TryAdd(new CoordinateWithHashCode(x, y, 7), string.Empty);
            data2.TryAdd(new CoordinateWithHashCodeAndEquals(x, y, 7), string.Empty);
        }
    }

    [Benchmark]
    public string GetItemWithHashSet()
    {
        for (var i = 0; i < 1000; i++) data.TryGetValue(new CoordinateWithHashCode(i, i, 7), out var value);

        return string.Empty;
    }

    [Benchmark]
    public string GetItemWithHashSetAndEquals()
    {
        for (var i = 0; i < 1000; i++)
            data2.TryGetValue(new CoordinateWithHashCodeAndEquals(i, i, 7), out var value);

        return string.Empty;
    }
}

public struct Coordinate : IEquatable<Coordinate>
{
    public Coordinate(int x, int y, sbyte z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public sbyte Z { get; set; }

    public bool Equals([AllowNull] Coordinate other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }
}

public struct CoordinateWithHashCode : IEquatable<CoordinateWithHashCode>
{
    public CoordinateWithHashCode(int x, int y, sbyte z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public sbyte Z { get; set; }

    public bool Equals([AllowNull] CoordinateWithHashCode other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}

public struct CoordinateWithHashCodeAndEquals : IEquatable<CoordinateWithHashCodeAndEquals>
{
    public CoordinateWithHashCodeAndEquals(int x, int y, sbyte z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public sbyte Z { get; set; }

    public bool Equals([AllowNull] CoordinateWithHashCodeAndEquals other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object other)
    {
        return other is CoordinateWithHashCodeAndEquals o && Equals(o);
    }

    public override int GetHashCode()
    {
        return HashHelper.START
            .CombineHashCode(X)
            .CombineHashCode(Y)
            .CombineHashCode(Z);
    }
}