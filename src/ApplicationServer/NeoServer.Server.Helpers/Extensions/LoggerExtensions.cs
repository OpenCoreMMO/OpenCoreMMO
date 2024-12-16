using System;
using System.Diagnostics;
using System.Linq;
using Serilog;

namespace NeoServer.Server.Helpers.Extensions;

public static class LoggerExtensions
{
    private static readonly Stopwatch Sw = new();

    public static void Step<T>(this ILogger logger, string beforeMessage, string afterMessage, Func<T> func,
        params object[] @params)
    {
        var lastRow = GetCursorTop();

        if (@params is null || @params.Length == 0)
            logger.Information(beforeMessage);
        else
            logger.Information(beforeMessage, @params);

        Sw.Restart();

        var result = func();

        var currentRow = GetCursorTop();
        SetCursorPosition(0, lastRow);
        if (@params is null || @params.Length == 0)
            logger.Information($"{afterMessage} in {{elapsed}} secs", result,
                Math.Round(Sw.ElapsedMilliseconds / 1000d, 2));
        else
            logger.Information($"{afterMessage} in {{elapsed}} secs",
                @params.Concat(new object[] { Math.Round(Sw.ElapsedMilliseconds / 1000d, 2) }).ToArray());

       SetCursorPosition(0, currentRow);
    }

    public static void Step(this ILogger logger, string beforeMessage, string afterMessage, Action action,
        params object[] @params)
    {
        var lastRow = GetCursorTop();

        if (@params is null || @params.Length == 0)
            logger.Information(beforeMessage);
        else
            logger.Information(beforeMessage, @params);

        Sw.Restart();

        action();

        var currentRow = GetCursorTop();
        SetCursorPosition(0, lastRow);
        if (@params is null || @params.Length == 0)
            logger.Information($"{afterMessage} in {{elapsed}} secs",
                Math.Round(Sw.ElapsedMilliseconds / 1000d, 2));
        else
            logger.Information($"{afterMessage} in {{elapsed}} secs",
                @params.Concat(new object[] { Math.Round(Sw.ElapsedMilliseconds / 1000d, 2) }).ToArray());

        SetCursorPosition(0, currentRow);
    }

    public static void Step(this ILogger logger, string beforeMessage, string afterMessage, Func<object[]> action)
    {
        var lastRow = GetCursorTop();

        logger.Information(beforeMessage);

        Sw.Restart();

        var @params = action();

        var currentRow = GetCursorTop();
        SetCursorPosition(0, lastRow);

        if (@params is null || @params.Length == 0)
            logger.Information($"{afterMessage} in {{elapsed}} secs",
                Math.Round(Sw.ElapsedMilliseconds / 1000d, 2));
        else
            logger.Information($"{afterMessage} in {{elapsed}} secs",
                @params.Concat(new object[] { Math.Round(Sw.ElapsedMilliseconds / 1000d, 2) }).ToArray());

        SetCursorPosition(0, currentRow);
    }

    private static int GetCursorTop()
    {
        var cursor = 0;
        try { cursor = Console.CursorTop; }
        catch { }

        return cursor;
    }

    private static void SetCursorPosition(int position, int lastRow)
    {
        try { Console.SetCursorPosition(position, lastRow);}
        catch { }
    }
}