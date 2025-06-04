using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Texts;

namespace NeoServer.Domain.Common.Effects.Parsers;

public static class DamageTextColorParser
{
    public static TextColor Parse(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Fire => TextColor.Orange,
            DamageType.Energy => TextColor.Purple,
            DamageType.Melee => TextColor.Red,
            DamageType.Physical => TextColor.Red,
            DamageType.MagicalPhysical => TextColor.Red,
            DamageType.ManaDrain => TextColor.Blue,
            DamageType.FireField => TextColor.Orange,
            DamageType.Earth => TextColor.LightGreen,
            DamageType.Death => TextColor.DarkRed,
            DamageType.LifeDrain => TextColor.DarkRed,
            DamageType.Ice => TextColor.LightBlue,
            DamageType.Holy => TextColor.Yellow,
            _ => TextColor.Red
        };
    }

    public static TextColor Parse(DamageType damageType, ICreature creature)
    {
        if (damageType is DamageType.Melee && creature is IMonster monster)
            return monster.Metadata.Race switch
            {
                Race.Venom => TextColor.LightGreen,
                Race.Fire => TextColor.Orange,
                Race.Undead => TextColor.Grey,
                Race.Energy => TextColor.ElectricPurple,
                _ => TextColor.Red
            };

        return Parse(damageType);
    }
}