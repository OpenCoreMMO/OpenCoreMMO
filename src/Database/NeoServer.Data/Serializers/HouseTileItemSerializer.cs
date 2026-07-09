using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Items;

namespace NeoServer.Data.Serializers;

public static class HouseTileItemSerializer
{
    private const byte FlagActionId     = 1 << 0;
    private const byte FlagUniqueId     = 1 << 1;
    private const byte FlagCharges      = 1 << 2;
    private const byte FlagDecay        = 1 << 3;
    private const byte FlagFluidType    = 1 << 4;
    private const byte FlagTextData     = 1 << 5;
    private const byte FlagDescription  = 1 << 6;
    private const byte FlagContainer    = 1 << 7;

    public static byte[] Serialize(IEnumerable<IItem> items)
    {
        using var ms = new MemoryStream(capacity: 256);
        using var writer = new BinaryWriter(ms);

        ushort count = 0;
        var itemBuffer = new MemoryStream();
        var itemWriter = new BinaryWriter(itemBuffer);

        foreach (var item in items)
        {
            if (item is null) continue;
            WriteItem(itemWriter, item);
            count++;
        }

        writer.Write(count);
        writer.Write(itemBuffer.ToArray());

        return ms.ToArray();
    }

    public static List<IItem> Deserialize(byte[] data, IItemFactory itemFactory, Location location)
    {
        var items = new List<IItem>();

        if (data is null || data.Length == 0) return items;

        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        var count = reader.ReadUInt16();
        for (var i = 0; i < count; i++)
        {
            var item = ReadItem(reader, itemFactory, location);
            if (item is not null) items.Add(item);
        }

        return items;
    }

    private static void WriteItem(BinaryWriter writer, IItem item)
    {
        var flags = BuildFlags(item);
        writer.Write(item.Metadata.ServerId);
        writer.Write(item.Amount > 0 ? (ushort)item.Amount : (ushort)1);
        writer.Write(flags);

        if ((flags & FlagActionId) != 0)    writer.Write(item.ActionId);
        if ((flags & FlagUniqueId) != 0)    writer.Write((uint)item.UniqueId);
        if ((flags & FlagCharges) != 0)     writer.Write(GetCharges(item));
        if ((flags & FlagDecay) != 0)       WriteDecay(writer, item);
        if ((flags & FlagFluidType) != 0)   writer.Write(GetFluidType(item));
        if ((flags & FlagTextData) != 0)    WriteTextData(writer, item);
        if ((flags & FlagDescription) != 0) WriteString(writer, GetDescription(item));

        if ((flags & FlagContainer) != 0)
        {
            var container = (IContainer)item;
            var children = container.Items;
            writer.Write((ushort)(children?.Count ?? 0));
            if (children is not null)
            {
                foreach (var child in children)
                {
                    WriteItem(writer, child);
                }
            }
        }
    }

    private static IItem ReadItem(BinaryReader reader, IItemFactory itemFactory, Location location)
    {
        var serverId = reader.ReadUInt16();
        var amount = reader.ReadUInt16();
        var flags = reader.ReadByte();

        var itemAttributes = new Dictionary<ItemAttribute, IConvertible>();
        var itemCustomAttributes = new Dictionary<string, IConvertible>();
        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>();
        var itemTypeCustomAttributes = new Dictionary<string, IConvertible>();

        if (amount > 1)
            itemAttributes[ItemAttribute.Count] = (byte)amount;

        if ((flags & FlagActionId) != 0)
            itemAttributes[ItemAttribute.ActionId] = reader.ReadUInt16();

        if ((flags & FlagUniqueId) != 0)
            itemAttributes[ItemAttribute.UniqueId] = reader.ReadUInt32();

        if ((flags & FlagCharges) != 0)
            itemAttributes[ItemAttribute.Charges] = reader.ReadUInt16();

        if ((flags & FlagDecay) != 0)
            ReadDecay(reader, itemAttributes);

        if ((flags & FlagFluidType) != 0)
            itemAttributes[ItemAttribute.FluidType] = reader.ReadUInt16();

        if ((flags & FlagTextData) != 0)
            ReadTextData(reader, itemAttributes);

        if ((flags & FlagDescription) != 0)
            itemAttributes[ItemAttribute.Description] = ReadString(reader);

        List<IItem> children = null;
        if ((flags & FlagContainer) != 0)
        {
            var childCount = reader.ReadUInt16();
            if (childCount > 0)
            {
                children = new List<IItem>(childCount);
                for (var i = 0; i < childCount; i++)
                {
                    var child = ReadItem(reader, itemFactory, location);
                    if (child is not null) children.Add(child);
                }
            }
        }

        return itemFactory.Create(
            serverId,
            location,
            itemTypeAttributes,
            itemTypeCustomAttributes,
            itemAttributes,
            itemCustomAttributes,
            children
        );
    }

    private static byte BuildFlags(IItem item)
    {
        byte flags = 0;

        if (item.ActionId != 0)        flags |= FlagActionId;
        if (item.UniqueId != 0)        flags |= FlagUniqueId;
        if (HasCharges(item))          flags |= FlagCharges;
        if (HasDecay(item))            flags |= FlagDecay;
        if (HasFluidType(item))        flags |= FlagFluidType;
        if (HasTextData(item))         flags |= FlagTextData;
        if (HasDescription(item))      flags |= FlagDescription;
        if (item.IsContainer)          flags |= FlagContainer;

        return flags;
    }

    private static bool HasCharges(IItem item) =>
        item.Attributes.HasAttribute(ItemAttribute.Charges);

    private static ushort GetCharges(IItem item) =>
        item.Attributes.GetAttribute<ushort>(ItemAttribute.Charges);

    private static bool HasDecay(IItem item) =>
        item.Attributes.HasAttribute(ItemAttribute.DecayTo) ||
        item.Attributes.HasAttribute(ItemAttribute.Duration) ||
        item.Attributes.HasAttribute(ItemAttribute.DecayState);

    private static void WriteDecay(BinaryWriter writer, IItem item)
    {
        writer.Write(item.Attributes.TryGetAttribute<ushort>(ItemAttribute.DecayTo, out var decayTo) ? decayTo : (ushort)0);
        writer.Write(item.Attributes.TryGetAttribute<uint>(ItemAttribute.Duration, out var duration) ? duration : 0u);
        writer.Write(item.Attributes.TryGetAttribute<uint>(ItemAttribute.DecayState, out var elapsed) ? elapsed : 0u);
    }

    private static void ReadDecay(BinaryReader reader, Dictionary<ItemAttribute, IConvertible> attributes)
    {
        var decayTo = reader.ReadUInt16();
        var duration = reader.ReadUInt32();
        var elapsed = reader.ReadUInt32();

        if (decayTo != 0)   attributes[ItemAttribute.DecayTo] = decayTo;
        if (duration != 0)  attributes[ItemAttribute.Duration] = duration;
        if (elapsed != 0)   attributes[ItemAttribute.DecayState] = elapsed;
    }

    private static bool HasFluidType(IItem item) =>
        item.Attributes.HasAttribute(ItemAttribute.FluidType);

    private static ushort GetFluidType(IItem item) =>
        item.Attributes.GetAttribute<ushort>(ItemAttribute.FluidType);

    private static bool HasTextData(IItem item) =>
        item.Attributes.HasAttribute(ItemAttribute.Text) ||
        item.Attributes.HasAttribute(ItemAttribute.Writer) ||
        item.Attributes.HasAttribute(ItemAttribute.Date);

    private static void WriteTextData(BinaryWriter writer, IItem item)
    {
        var text = item.Attributes.GetAttribute(ItemAttribute.Text) ?? string.Empty;
        var writerName = item.Attributes.GetAttribute(ItemAttribute.Writer) ?? string.Empty;
        var date = item.Attributes.TryGetAttribute<uint>(ItemAttribute.Date, out var d) ? d : 0u;

        WriteString(writer, text);
        WriteString(writer, writerName);
        writer.Write(date);
    }

    private static void ReadTextData(BinaryReader reader, Dictionary<ItemAttribute, IConvertible> attributes)
    {
        var text = ReadString(reader);
        var writerName = ReadString(reader);
        var date = reader.ReadUInt32();

        if (text.Length > 0)       attributes[ItemAttribute.Text] = text;
        if (writerName.Length > 0) attributes[ItemAttribute.Writer] = writerName;
        if (date != 0)             attributes[ItemAttribute.Date] = date;
    }

    private static bool HasDescription(IItem item) =>
        item.Attributes.HasAttribute(ItemAttribute.Description);

    private static string GetDescription(IItem item) =>
        item.Attributes.GetAttribute(ItemAttribute.Description) ?? string.Empty;

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadUInt16();
        if (length == 0) return string.Empty;
        var bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }
}
