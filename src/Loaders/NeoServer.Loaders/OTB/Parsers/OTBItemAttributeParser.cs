using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Loaders.OTB.Enums;

namespace NeoServer.Loaders.OTB.Parsers;

/// <summary>
///     Responsible for parse the item attributes coming from OTB structure
/// </summary>
public sealed class OtbParsingItemAttribute
{
    /// <summary>
    ///     Creates OTBParsingItemAttribute instance
    /// </summary>
    /// <param name="stream"></param>
    public OtbParsingItemAttribute(OtbParsingStream stream)
    {
        if (stream.IsNull()) return;
        while (!stream.IsOver) Parse(stream);
    }

    /// <summary>
    ///     Dictionary containing the otb item attributes and its respective values
    /// </summary>
    /// <value></value>
    public Dictionary<OtbItemAttribute, IConvertible> Attributes { get; } = new();

    private void Parse(OtbParsingStream stream)
    {
        var attribute = (OtbItemAttribute)stream.ReadByte();
        var dataLength = stream.ReadUInt16();

        switch (attribute)
        {
            case OtbItemAttribute.ServerId:
                dataLength.ThrowIfNotEqualsTo<ushort>(sizeof(ushort));

                Attributes.TryAdd(OtbItemAttribute.ServerId, stream.ReadUInt16());
                break;

            case OtbItemAttribute.ClientId:
                dataLength.ThrowIfNotEqualsTo<ushort>(sizeof(ushort));
                Attributes.TryAdd(OtbItemAttribute.ClientId, stream.ReadUInt16());
                break;

            case OtbItemAttribute.Speed:
                dataLength.ThrowIfNotEqualsTo<ushort>(sizeof(ushort));
                Attributes.TryAdd(OtbItemAttribute.Speed, stream.ReadUInt16());

                break;
            case OtbItemAttribute.Light2:
                //todo validation

                Attributes.TryAdd(OtbItemAttribute.LightLevel, (byte)stream.ReadUInt16());
                Attributes.TryAdd(OtbItemAttribute.LightColor, (byte)stream.ReadUInt16());
                break;
            case OtbItemAttribute.TopOrder:
                dataLength.ThrowIfNotEqualsTo<ushort>(sizeof(byte));
                Attributes.TryAdd(OtbItemAttribute.TopOrder, stream.ReadByte());
                break;
            case OtbItemAttribute.WareId:
                dataLength.ThrowIfNotEqualsTo<ushort>(sizeof(ushort));
                Attributes.TryAdd(OtbItemAttribute.WareId, stream.ReadUInt16());
                break;

            default:
                //skip unkown attributes
                stream.Skip(dataLength);
                break;
        }
    }
}