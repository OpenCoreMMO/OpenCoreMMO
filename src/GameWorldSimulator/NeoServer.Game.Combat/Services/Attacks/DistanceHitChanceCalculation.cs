using System;

namespace NeoServer.Game.Combat.Services.Attacks;

public static class DistanceHitChanceCalculation
{
    public static byte CalculateFor2Hands(ushort skill, byte range)
    {
        return (byte)(range switch
        {
            1 => Math.Min(90, 1.2 * skill + 1),
            2 => Math.Min(90, 3.2 * skill),
            3 => Math.Min(90, 2 * skill),
            4 => Math.Min(90, 1.55 * skill),
            5 => Math.Min(90, 1.2 * skill + 1),
            6 => Math.Min(90, (int)skill),
            _ => 0
        });
    }

    public static byte CalculateFor1Hand(ushort skill, byte range)
    {
        return (byte)(range switch
        {
            1 => Math.Min(75, skill + 1),
            2 => Math.Min(75, 2.4 * skill + 8),
            3 => Math.Min(75, 1.55 * skill + 6),
            4 => Math.Min(75, 1.25 * skill + 3),
            5 => Math.Min(75, skill + 1),
            6 => Math.Min(75, 0.8 * skill + 3),
            7 => Math.Min(75, 0.7 * skill + 2),
            _ => 0
        });
    }
}