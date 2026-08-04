using System.Collections;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.World.Structures;

public class TileStack<T>(int size = 10) : IEnumerable<T>
    where T : IThing
{
    private readonly List<T> _items = new(size);

    public int Count => _items.Count;

    public IReadOnlyList<T> Values => _items;

    public IEnumerator<T> GetEnumerator()
    {
        return Enumerable.Reverse(_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Push(T item)
    {
        _items.Add(item);
    }

    public void Insert(T item, T beforeItem)
    {
        var index = _items.IndexOf(beforeItem);
        if (index < 0) return;
        _items.Insert(index, item);
    }

    public T Pop()
    {
        if (_items.Count == 0) return default;

        var temp = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return temp;
    }

    public void Remove(int itemAtPosition)
    {
        if (itemAtPosition < 0) return;
        _items.RemoveAt(itemAtPosition);
    }

    public bool TryPeek(out T item)
    {
        item = default;

        if (_items.Count == 0) return false;
        item = _items[^1];
        return true;
    }

    public bool TryPop(out T item)
    {
        item = default;

        if (_items.Count == 0) return false;
        item = Pop();
        return true;
    }

    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        if (index < 0) return false;

        Remove(index);
        return true;
    }
}