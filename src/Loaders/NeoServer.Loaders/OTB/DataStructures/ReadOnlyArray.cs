using System;
using System.Collections;
using System.Collections.Generic;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Loaders.OTB.DataStructures;

public static class ArrayExtensions
{
    /// <summary>
    ///     This is a convenience method to get a generic enumrator from an array.
    /// </summary>
    public static IEnumerator<T> GetGenericEnumerator<T>(this T[] array)
    {
        return ((IEnumerable<T>)array).GetEnumerator();
    }
}

public sealed class ReadOnlyArray<T> : IReadOnlyList<T>
{
    private readonly T[] _items;

    private ReadOnlyArray(T[] items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _items = items;
    }

    /// <summary>
    ///     Returns the element at <paramref name="index" />-th position.
    /// </summary>
    public T this[int index] => _items[index];

    /// <summary>
    ///     Returns the number of elements the array can hold.
    /// </summary>
    public int Count => _items.Length;

    /// <summary>
    ///     Returns a generic enumerator for the array.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetGenericEnumerator();
    }

    /// <summary>
    ///     Returns a non-generic enumerator for the array.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ReadOnlyArray{T}" /> to wrap the provided array.
    ///     Since this is just a view of the actual array, if the underlying array is mutated so are the values
    ///     returned by the methods of this class.
    /// </summary>
    public static ReadOnlyArray<T> WrapCollection(T[] items)
    {
        return items.IsNull() ? null : new ReadOnlyArray<T>(items);
    }
}