using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTBM.Enums;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public readonly struct ItemNodeAttributeValue
{
    public ItemNodeAttribute AttributeName { get; }
    public IConvertible Value { get; }
    public IEnumerable<CustomAttribute> CustomAttributes { get; }

    public ItemNodeAttributeValue(ItemNodeAttribute attribute, OtbParsingStream stream)
    {
        Value = null;
        AttributeName = ItemNodeAttribute.None;
        CustomAttributes = new List<CustomAttribute>();

        AttributeName = attribute;

        switch (attribute)
        {
            case ItemNodeAttribute.Count:
            case ItemNodeAttribute.RuneCharges:
                Value = stream.ReadByte();
                break;
            case ItemNodeAttribute.ActionId:
            case ItemNodeAttribute.UniqueId:
            case ItemNodeAttribute.Charges:
            case ItemNodeAttribute.ExtraDefense:
            case ItemNodeAttribute.DepotId:
                Value = stream.ReadUInt16();
                break;
            case ItemNodeAttribute.Text:
            case ItemNodeAttribute.WrittenBy:
            case ItemNodeAttribute.Description:
            case ItemNodeAttribute.Name:
            case ItemNodeAttribute.Article:
            case ItemNodeAttribute.PluralName:
                Value = stream.ReadString();
                break;
            case ItemNodeAttribute.WrittenDate:
            case ItemNodeAttribute.Weight:
            case ItemNodeAttribute.Attack:
            case ItemNodeAttribute.Defense:
            case ItemNodeAttribute.Armor:
            case ItemNodeAttribute.HitChance:
            case ItemNodeAttribute.ShootRange:
            case ItemNodeAttribute.DecayTo:
            case ItemNodeAttribute.SleeperGUID:
            case ItemNodeAttribute.SleepStart:
            case ItemNodeAttribute.ContainerItems:
                Value = stream.ReadUInt32();
                break;

            case ItemNodeAttribute.Duration:
                Value = Math.Max(0, stream.ReadUInt32());
                break;
            case ItemNodeAttribute.DecayingState:
                Value = stream.ReadByte();

                if ((ItemDecayingState)Value != ItemDecayingState.False) Value = ItemDecayingState.Pending;
                break;

            case ItemNodeAttribute.HouseDoorId:
                Value = stream.ReadByte();
                break;
            case ItemNodeAttribute.TeleportDestination:
                if (!stream.CanReadNextBytes(5)) break;
                Value = new Location(stream.ReadUInt16(), stream.ReadUInt16(), stream.ReadByte());
                break;

            case ItemNodeAttribute.CustomAttributes:
                throw new NotImplementedException(); //todo
                // var size = stream.ReadUInt64();
                // for (ulong i = 0; i < size; i++)
                // {
                //     CustomAttributes = new List<CustomAttribute>();
                //
                //     ((List<CustomAttribute>)Value).Add(new CustomAttribute(stream));
                // }
                //
                // break;
        }
    }
}