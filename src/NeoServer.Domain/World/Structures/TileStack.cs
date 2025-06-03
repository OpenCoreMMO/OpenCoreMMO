using System.Collections;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.World.Structures;

public class TileStack<T> : IEnumerable<T> where T : IThing
{
    private readonly List<T> items;

    public TileStack(int size = 10)
    {
        items = new List<T>(size);
    }

    public int Count => items.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return Enumerable.Reverse(items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Push(T item)
    {
        items.Add(item);
    }

    public T Pop()
    {
        if (!items.Any()) return default;

        var temp = items[^1];
        items.RemoveAt(items.Count - 1);
        return temp;
    }

    public void Remove(int itemAtPosition)
    {
        if (itemAtPosition < 0) return;
        items.RemoveAt(itemAtPosition);
    }

    public bool TryPeek(out T item)
    {
        item = default;

        if (!items.Any()) return false;
        item = items[^1];
        return true;
    }

    public bool TryPop(out T item)
    {
        item = default;

        if (!items.Any()) return false;
        item = Pop();
        return true;
    }

    public bool Remove(T item)
    {
        var index = items.IndexOf(item);
        if (index < 0) return false;

        Remove(index);
        return true;
    }
}