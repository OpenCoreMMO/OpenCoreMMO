using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Common.Contracts.Items;


public delegate void ItemDelete(IItem item);

public delegate void ItemRemove(IItem item, IThing from);

public interface IItem : IThing, IHasDecay
{
    // Define these constants at the top of your file or in a suitable static class
    const byte CLIENTFLUID_EMPTY = 0x00;
    const byte CLIENTFLUID_BLUE = 0x01;
    const byte CLIENTFLUID_RED = 0x02;
    const byte CLIENTFLUID_BROWN_1 = 0x03;
    const byte CLIENTFLUID_GREEN = 0x04;
    const byte CLIENTFLUID_YELLOW = 0x05;
    const byte CLIENTFLUID_WHITE = 0x06;
    const byte CLIENTFLUID_PURPLE = 0x07;

    // Market special item IDs (similar to otclientv8's MarketRequest enum)
    const ushort MARKET_MYOFFERS = 0xFFFE; // 65534 - My Offers
    const ushort MARKET_MYHISTORY = 0xFF01; // 65281 - My History

    /// <summary>
    ///     Item metadata. Contains a lot of information about item
    /// </summary>
    IItemType Metadata { get; }
    ItemAttributeList Attributes { get; }

    ushort ActionId { get; }
    uint UniqueId { get; }
    string Article { get; }
    string Plural { get; }

    float Weight { get; }

    ushort AttackPower { get; }

    ushort Defense { get; }

    ushort ExtraDefense { get; }

    ushort Armor { get; }

    sbyte ExtraHitChance { get; }

    byte Range { get; }

    public bool IsDeleted { get; }
    IThing Owner { get; }

    IThing Parent { get; }

    string InspectionText => string.Empty;
    string CloseInspectionText => string.Empty;
    ushort ClientId => Metadata.ClientId;
    ushort ServerId => Metadata.ServerId;
    ushort CanTransformTo => Metadata.Attributes.GetTransformationItem();
    bool CanBeMoved => Metadata.HasFlag(ItemFlag.Movable);
    bool IsBlockeable => Metadata.HasFlag(ItemFlag.Unpassable);
    bool IsTransformable => CanTransformTo != default;
    bool BlockPathFinding => Metadata.HasFlag(ItemFlag.BlockPathFind);
    bool IsCumulative => Metadata.HasFlag(ItemFlag.Stackable);

    bool IsAlwaysOnTop => Metadata.HasFlag(ItemFlag.AlwaysOnTop) ||
                          Metadata.HasFlag(ItemFlag.Hangable);

    bool CanHang => Metadata.HasFlag(ItemFlag.Horizontal) || Metadata.HasFlag(ItemFlag.Vertical);

    bool IsPickupable => Metadata.HasFlag(ItemFlag.Pickupable);
    bool IsUsable => Metadata.HasFlag(ItemFlag.Usable);
    bool IsAntiProjectile => Metadata.HasFlag(ItemFlag.BlockProjectTile);
    bool IsContainer => Metadata.Group == ItemGroup.Container;
    bool IsTeleport => Metadata.Group == ItemGroup.Teleport;

    bool AllowFarUse => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.AllowFarUse);

    FloorChangeDirection FloorDirection => Metadata.Attributes.GetFloorChangeDirection();

    bool HasDecayBehavior
    {
        get
        {
            var hasShowDuration =
                Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ShowDuration, out _);
            var hasDuration = Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.Duration, out _);

            return hasShowDuration || hasDuration;
        }
    }

    public string FullName
    {
        get
        {
            if (Attributes.HasAttribute(ItemAttribute.Article))
            {
               return string.IsNullOrWhiteSpace(Attributes.GetAttribute(ItemAttribute.Article))
                ? $"{Attributes.GetAttribute(ItemAttribute.Name)}"
                : $"{Attributes.GetAttribute(ItemAttribute.Article)} {Attributes.GetAttribute(ItemAttribute.Name)}";
            }

            return Metadata.FullName;
        }
    }

    void UpdateMetadata(IItemType newMetadata);
    void MarkAsDeleted();

    Span<byte> GetRaw()
    {
        var bytes = new List<byte>();
        var it = Metadata;
        byte count = Amount; // Assuming Amount is the stack count or fluid type

        // Fluid map as per common Tibia/Otserv conventions
        // Adjust values as needed for your protocol
        byte[] fluidMap = new byte[]
        {
            CLIENTFLUID_EMPTY,
            CLIENTFLUID_BLUE,
            CLIENTFLUID_RED,
            CLIENTFLUID_BROWN_1,
            CLIENTFLUID_GREEN,
            CLIENTFLUID_YELLOW,
            CLIENTFLUID_WHITE,
            CLIENTFLUID_PURPLE
        };

        // Handle special market IDs by replacing them with a valid item ID
        ushort clientIdToSend = ClientId;
        if (ClientId == MARKET_MYHISTORY || ClientId == MARKET_MYOFFERS)
        {
            // Use a valid bag/container ID instead of the special market ID
            // This prevents "unable to create item with invalid id" errors
            clientIdToSend = 1987; // Common bag ID used in Tibia
        }

        // Add ClientId (ushort, little-endian)
        bytes.AddRange(BitConverter.GetBytes(clientIdToSend));

        bytes.Add(0xFF); // MARK_UNMARKED

        if (it.IsStackable())
        {
            bytes.Add(count);
        }
        else if (it.IsSplash() || it.IsFluidContainer())
        {
            bytes.Add(fluidMap[count & 7]);
        }

        if (it.IsAnimation())
            bytes.Add(0xFE);

        return new Span<byte>(bytes.ToArray());
    }

    void SetOwner(IThing owner);
    event ItemDelete OnDeleted;
    void SetParent(IThing parent);
    event ItemRemove OnRemoved;
    void OnItemRemoved(IThing from);

    ushort GetSubType()
    {
        if (Metadata.HasFlag(ItemFlag.LiquidContainer))
        {
            var fluidType = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.ContainerLiquidType);
            return fluidType;
        }

        if (Metadata.HasFlag(ItemFlag.Stackable))
        {
            var count = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.Count);
            return count;
        }

        var charges = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.Charges);
        return charges;
    }
}