using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures;

namespace NeoServer.Loaders.Monsters.Converters;

public class MonsterImmunityConverter
{
    public static ushort Convert(MonsterData data)
    {
        if (data.Immunities is null) return default;

        ushort flag = 0;

        foreach (var immunity in data.Immunities)
            if (HasImmunity(immunity, "lifedrain"))
                flag |= (ushort)Immunity.LifeDrain;
            else if (HasImmunity(immunity, "paralyze"))
                flag |= (ushort)Immunity.Paralysis;
            else if (HasImmunity(immunity, "invisible"))
                flag |= (ushort)Immunity.Invisibility;
            else if (HasImmunity(immunity, "death"))
                flag |= (ushort)Immunity.Death;
            else if (HasImmunity(immunity, "fire"))
                flag |= (ushort)Immunity.Fire;
            else if (HasImmunity(immunity, "ice"))
                flag |= (ushort)Immunity.Ice;
            else if (HasImmunity(immunity, "drown"))
                flag |= (ushort)Immunity.Drown;
            else if (HasImmunity(immunity, "drunk"))
                flag |= (ushort)Immunity.Drunkenness;
            else if (HasImmunity(immunity, "earth"))
                flag |= (ushort)Immunity.Earth;
            else if (HasImmunity(immunity, "energy"))
                flag |= (ushort)Immunity.Energy;
            else if (HasImmunity(immunity, "poison"))
                flag |= (ushort)Immunity.Earth;
            else if (HasImmunity(immunity, "physical"))
                flag |= (ushort)Immunity.Physical;
            else if (HasImmunity(immunity, "holy"))
                flag |= (ushort)Immunity.Holy;

        return flag;
    }

    private static bool HasImmunity(KeyValuePair<string, byte> immunity, string key)
    {
        return immunity.Key.Contains(key, StringComparison.InvariantCultureIgnoreCase) && immunity.Value == 1;
    }
}