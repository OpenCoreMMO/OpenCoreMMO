using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures.Monsters;
using NeoServer.Domain.Creatures.Monster.Summon;

namespace NeoServer.Loaders.Monsters.Converters;

public static class MonsterSummonConverter
{
    public static (int, IMonsterSummon[]) Convert(MonsterData data)
    {
        if (data.Summon is null || data.Summon.MaxSummons == 0) return (0, null);

        var summons = new List<IMonsterSummon>(data.Summon.Summons.Count);

        foreach (var summon in data.Summon.Summons)
        {
            if (string.IsNullOrWhiteSpace(summon.Name) || summon.Chance <= 0 || summon.Max <= 0) continue;

            summons.Add(new MonsterSummon(summon.Name, summon.Interval == 0 ? 1000 : summon.Interval,
                (byte)summon.Chance, (byte)summon.Max));
        }

        return (data.Summon.MaxSummons, summons.ToArray());
    }
}