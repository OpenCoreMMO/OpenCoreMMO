using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures;

namespace NeoServer.Domain.Common.Parsers;

public static class DamageTypeParser
{
    public static string Parse(DamageType type)
    {
        return type switch
        {
            DamageType.Melee => "melee",
            DamageType.Physical => "physical",
            DamageType.Energy => "energy",
            DamageType.Fire => "fire",
            DamageType.FireField => "fire",
            DamageType.ManaDrain => "manadrain",
            DamageType.Earth => "earth",
            DamageType.Drown => "drown",
            DamageType.Ice => "ice",
            DamageType.Holy => "holy",
            DamageType.Death => "death",
            DamageType.LifeDrain => "lifedrain",
            DamageType.All => "all",
            DamageType.Elemental => "elemental",
            _ => "physical"
        };
    }

    public static DamageType Parse(string type)
    {
        return type switch
        {
            "melee" => DamageType.Melee,
            "physical" => DamageType.Physical,
            "energy" => DamageType.Energy,
            "fire" => DamageType.Fire,
            "firefield" => DamageType.FireField,
            "manadrain" => DamageType.ManaDrain,
            "firearea" => DamageType.Fire,
            "poison" => DamageType.Earth,
            "earth" => DamageType.Earth,
            "bleed" => DamageType.Physical,
            "drown" => DamageType.Drown,
            "ice" => DamageType.Ice,
            "holy" => DamageType.Holy,
            "death" => DamageType.Death,
            "lifedrain" => DamageType.LifeDrain,
            "mortarea" => DamageType.Death,
            _ => DamageType.Melee
        };
    }

    public static Immunity ToImmunity(this DamageType type)
    {
        return type switch
        {
            DamageType.Melee => Immunity.Physical,
            DamageType.Physical => Immunity.Physical,
            DamageType.Energy => Immunity.Energy,
            DamageType.Fire => Immunity.Fire,
            DamageType.FireField => Immunity.Fire,
            DamageType.ManaDrain => Immunity.ManaDrain,
            DamageType.Earth => Immunity.Earth,
            DamageType.Drown => Immunity.Drown,
            DamageType.Ice => Immunity.Ice,
            DamageType.Holy => Immunity.Holy,
            DamageType.Death => Immunity.Death,
            DamageType.LifeDrain => Immunity.LifeDrain,
            DamageType.Drunk => Immunity.Drunkenness,
            _ => Immunity.None
        };
    }
}