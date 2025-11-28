using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class LiquidPool : BaseItem, ILiquid
{
    public LiquidPool(IItemType type, Location location, LiquidColor? color = null) : base(type, location)
    {
        if (color != null && color.HasValue)
            Attributes.SetAttribute(ItemAttribute.Count, GetLiquidColor(color.Value));
    }

    public bool IsLiquidPool => Metadata.Group == ItemGroup.Splash;
    public bool IsLiquidSource => Metadata.Flags.Contains(ItemFlag.LiquidSource);
    public bool IsLiquidContainer => Metadata.Group == ItemGroup.Fluid;
    public LiquidColor LiquidColor => GetLiquidColor();
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

    private LiquidColor GetLiquidColor()
    {
        if (!IsLiquidPool && !IsLiquidContainer) return 0x00;
        if (Attributes != null && Attributes.TryGetValue(ItemAttribute.Count, out var itemCount))
            return (LiquidColor)itemCount;
        if (Metadata.Attributes != null &&
            Metadata.Attributes.TryGetValue(ItemTypeAttribute.Count, out var itemTypeCount))
            return (LiquidColor)itemTypeCount;

        return LiquidColor.Empty;
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group == ItemGroup.Splash ||
               type.Flags.Contains(ItemFlag.LiquidSource) ||
               type.Group == ItemGroup.Fluid;
    }
}