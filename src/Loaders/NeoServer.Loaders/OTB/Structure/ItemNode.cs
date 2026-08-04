using System;
using System.Collections.Generic;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;

namespace NeoServer.Loaders.OTB.Structure;

public class ItemNode
{
    private readonly Dictionary<OtbItemAttribute, IConvertible> _attributes;

    /// <summary>
    ///     Creates ItemNode instance
    /// </summary>
    /// <param name="node"></param>
    public ItemNode(OtbNode node)
    {
        Type = node.Type;
        var stream = new OtbParsingStream(node.Data);

        Flags = stream.ReadUInt32();

        _attributes = new OtbParsingItemAttribute(stream).Attributes;
    }

    /// <summary>
    ///     Gets the item flags
    /// </summary>
    /// <value></value>
    public uint Flags { get; set; }

    /// <summary>
    ///     Gets the item's ServerId
    ///     In case of ServerId is between 30000 and 30100, subtract 30000
    /// </summary>
    public ushort ServerId
    {
        get
        {
            var value = GetValue<ushort>(OtbItemAttribute.ServerId);
            if (value is > 30000 and < 30100) value -= 30000;
            return value;
        }
    }

    public ushort ClientId => GetValue<ushort>(OtbItemAttribute.ClientId);

    public ushort Speed => GetValue<ushort>(OtbItemAttribute.Speed);

    public ushort WareId => GetValue<ushort>(OtbItemAttribute.WareId);

    public byte LightLevel => GetValue<byte>(OtbItemAttribute.LightLevel);
    public byte LightColor => GetValue<byte>(OtbItemAttribute.LightColor);
    public byte TopOrder => GetValue<byte>(OtbItemAttribute.TopOrder);

    /// <summary>
    ///     Gets the item's type
    ///     Used to set ItemType group and type
    /// </summary>
    /// <value></value>
    public NodeType Type { get; }

    private T GetValue<T>(OtbItemAttribute attribute)
    {
        if (_attributes.TryGetValue(attribute, out var value)) return (T)value;

        return default;
    }
}