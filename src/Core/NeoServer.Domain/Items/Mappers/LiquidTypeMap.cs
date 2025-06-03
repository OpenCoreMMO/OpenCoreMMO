using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Items.Mappers;

public class LiquidTypeMap
{
    private readonly Dictionary<byte, LiquidColor> types = new()
    {
        { 0, LiquidColor.Empty },
        { 1, LiquidColor.Blue },
        { 2, LiquidColor.Red },
        { 3, LiquidColor.Brown },
        { 4, LiquidColor.Green },
        { 5, LiquidColor.Yellow },
        { 6, LiquidColor.White },
        { 7, LiquidColor.Purple }
    };

    public LiquidColor this[byte value]
    {
        get
        {
            if (types.TryGetValue(value, out var color)) return color;
            return LiquidColor.Empty;
        }
    }
}