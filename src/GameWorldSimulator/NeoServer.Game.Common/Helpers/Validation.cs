using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoServer.Game.Common.Helpers;

public static class Validation
{
    public static bool IsNull<T>(this T value)
    {
        return value is null;
    }

    public static bool IsNotNull<T>(this T value)
    {
        return value is not null;
    }

    public static bool IsLessThanZero(this int value)
    {
        return value < 0;
    }

    public static bool IsLessThan<T>(this T value, T secondValue)
    {
        return Comparer<T>.Default.Compare(value, secondValue) < 0;
    }

    public static bool ThrowIfBiggerThan<T>(this T value, T secondValue)
    {
        return Comparer<T>.Default.Compare(value, secondValue) > 0;
    }

    public static bool ThrowIfNotEqualsTo<T>(this T value, T secondValue)
    {
        return Comparer<T>.Default.Compare(value, secondValue) != 0;
    }
}

public class Guard
{
    public static bool AnyNull(params object[] values)
    {
        foreach (var value in values)
            if (value.IsNull())
                return true;
        return false;
    }

    public static bool IsNull<T>(T value)
    {
        return value is null;
    }
    
    public static bool IsNullOrEmpty(ICollection value)
    {
        return value is null || value.Count == 0;
    }

    public static bool AnyNullOrEmpty(params ICollection[] values)
    {
        foreach (var value in values)
            if (value is null || value.Count == 0)
                return true;
        return false;
    }

    public static bool AnyNullOrEmpty(params string[] values)
    {
        foreach (var value in values)
            if (string.IsNullOrWhiteSpace(value))
                return true;
        return false;
    }

    public static bool AllNullOrEmpty(params string[] values)
    {
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                return false;
        return true;
    }
}