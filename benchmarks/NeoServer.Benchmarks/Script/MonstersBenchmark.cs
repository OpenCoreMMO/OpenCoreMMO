using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using NLua;

namespace NeoServer.Benchmarks.Script;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.ColdStart, 10)]
public class LuaBenchmark
{
    [Benchmark]
    public double MoonSharpReturnValue()
    {
        var script = new MoonSharp.Interpreter.Script();
        double sum = 0;
        for (var i = 0; i < 10; i++) sum += script.DoString($"return {i}").Number;
        return sum;
    }

    [Benchmark]
    public long NLuaReturnValue()
    {
        long sum = 0;
        var lua = new Lua();
        for (var i = 0; i < 10; i++) sum += (long)lua.DoString($"return {i}")[0];
        return sum;
    }

    [Benchmark]
    public double MoonSharpCallFunction()
    {
        var script = new MoonSharp.Interpreter.Script();
        script.DoString("function sum(a,b) return a + b end");

        double sum = 0;

        for (var i = 0; i < 10; i++) sum += script.Call(script.Globals["sum"], sum, 1).Number;

        return sum;
    }

    [Benchmark]
    public long NLuaCallFunction()
    {
        var lua = new Lua();
        lua.DoString("function sum(a,b) return a + b end");

        long sum = 0;
        for (var i = 0; i < 10; i++) sum += (long)lua.GetFunction("sum").Call(sum, i)[0];

        return sum;
    }
}