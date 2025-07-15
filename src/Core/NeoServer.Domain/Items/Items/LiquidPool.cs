using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Mappers;

namespace NeoServer.Domain.Items.Items;

public class LiquidPool : BaseItem, ILiquid
{
    public LiquidPool(IItemType type, Location location, LiquidColor color) : base(type, location)
    {
        LiquidColor = LiquidColor.Empty;
        LiquidColor = GetLiquidColor(color);
    }

    public LiquidPool(IItemType type, Location location,
        IDictionary<ItemTypeAttribute, IConvertible> attributes) : base(type, location)
    {
        LiquidColor = LiquidColor.Empty;
        LiquidColor = GetLiquidColor(attributes);
    }

    public bool IsLiquidPool => Metadata.Group == ItemGroup.Splash;
    public bool IsLiquidSource => Metadata.Flags.Contains(ItemFlag.LiquidSource);
    public bool IsLiquidContainer => Metadata.Group == ItemGroup.Fluid;
    public LiquidColor LiquidColor { get; }
    public ushort ClientId => Metadata.ClientId;

    public Span<byte> GetRaw()
    {
        Span<byte> cache = stackalloc byte[3];
        var idBytes = BitConverter.GetBytes(ClientId);

        cache[0] = idBytes[0];
        cache[1] = idBytes[1];
        cache[2] = (byte)LiquidColor;

        return cache.ToArray();
    }

    private LiquidColor GetLiquidColor(LiquidColor color)
    {
        if (!IsLiquidPool && !IsLiquidContainer) return 0x00;

        return color;
    }

    private LiquidColor GetLiquidColor(IDictionary<ItemTypeAttribute, IConvertible> attributes)
    {
        if (!IsLiquidPool && !IsLiquidContainer) return 0x00;
        if (attributes != null && attributes.TryGetValue(ItemTypeAttribute.Count, out var count))
            return new LiquidTypeMap()[(byte)count];
        return LiquidColor.Empty;
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group == ItemGroup.Splash ||
               type.Flags.Contains(ItemFlag.LiquidSource) ||
               type.Group == ItemGroup.Fluid;
    }
}