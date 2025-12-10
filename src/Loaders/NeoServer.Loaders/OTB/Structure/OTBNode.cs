using System;
using System.Collections.Generic;
using NeoServer.Loaders.OTB.DataStructures;
using NeoServer.Loaders.OTB.Enums;

namespace NeoServer.Loaders.OTB.Structure;

public class OtbNode
{
    private OtbNode[] _children;
    private int _childrenCount;
    private byte[] _data;
    private int _dataCount;

    /// <summary>
    ///     The type of the node.
    /// </summary>
    public readonly NodeType Type;

    /// <summary>
    ///     Creates a new instance of a <see cref="OtbNode" />.
    /// </summary>
    public OtbNode(NodeType type)
    {
        _children = new OtbNode[4]; // Pre-allocate small capacity
        _childrenCount = 0;
        _data = new byte[32]; // Pre-allocate reasonable size
        _dataCount = 0;
        Type = type;
    }

    /// <summary>
    ///     The children of this node.
    /// </summary>
    public ReadOnlyMemory<OtbNode> Children => new(_children, 0, _childrenCount);

    /// <summary>
    ///     The data of this node.
    /// </summary>
    public ReadOnlyMemory<byte> Data => new(_data, 0, _dataCount);

    /// <summary>
    ///     Adds child node
    /// </summary>
    /// <param name="node"></param>
    public void AddChild(OtbNode node)
    {
        if (_childrenCount >= _children.Length)
        {
            Array.Resize(ref _children, _children.Length * 2);
        }
        _children[_childrenCount++] = node;
    }

    /// <summary>
    ///     Adds byte to node's data
    /// </summary>
    /// <param name="b">The byte data to add</param>
    public void AddData(byte b)
    {
        if (_dataCount >= _data.Length)
        {
            Array.Resize(ref _data, _data.Length * 2);
        }
        _data[_dataCount++] = b;
    }

    /// <summary>
    ///     Adds multiple bytes to node's data efficiently
    /// </summary>
    /// <param name="bytes">The span of bytes to add</param>
    public void AddDataRange(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return;

        int requiredSize = _dataCount + bytes.Length;
        if (requiredSize > _data.Length)
        {
            int newCapacity = Math.Max(_data.Length * 2, requiredSize);
            Array.Resize(ref _data, newCapacity);
        }

        bytes.CopyTo(_data.AsSpan(_dataCount));
        _dataCount += bytes.Length;
    }
}