using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Items.Mappers;

public class LiquidTypeMap
{
    // Reverse mapping from enum values to keys
    private readonly Dictionary<LiquidColor, byte> reverseTypes;

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

    public LiquidTypeMap()
    {
        reverseTypes = types.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public LiquidColor this[byte value]
    {
        get
        {
            if (types.TryGetValue(value, out var color)) return color;
            return LiquidColor.Empty;
        }
    }

    public byte GetReverseLiquidColor(LiquidColor liquidColor)
    {
        //Get value from the reverse mapping
        if (reverseTypes.TryGetValue(liquidColor, out var value)) return value;
        return 0; // Default to 0 if not found
    }
}