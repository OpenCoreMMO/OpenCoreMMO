using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Monster.Summon;

namespace NeoServer.Domain.Creatures.Experience;

/// <summary>
///     Modifies the base experience of the monster that the player receives based on the portion of the damage they dealt.
/// </summary>
public class ProportionalExperienceModifier : IBaseExperienceModifier
{
    public string Name => "Proportional Damage";

    public uint GetModifiedBaseExperience(IPlayer player, IMonster monster, uint baseExperience)
    {
        var totalDamage = GetTotalMonsterDamage(monster);
        var playerDamage = GetTotalPlayerDamage(monster, player);
        var percentDamageFactor = playerDamage / (double)totalDamage;
        var experience = (uint)(baseExperience * percentDamageFactor);
        return experience;
    }

    public bool IsEnabled(IPlayer player, IMonster monster)
    {
        return true;
    }

    private static int GetTotalMonsterDamage(IMonster monster)
    {
        return monster.ReceivedDamages.TotalDamage;
    }

    private static int GetTotalPlayerDamage(IMonster monster, IPlayer player)
    {
        var damage = 0;
        foreach (var damageRecord in monster.ReceivedDamages)
        {
            if (damageRecord.Aggressor is not IPlayer playerAggressor) continue;
            if (playerAggressor != player &&
                (damageRecord.Aggressor as Summon)?.Master.CreatureId != player.CreatureId) continue;
            damage += damageRecord.Damage;
        }

        return damage;
    }
}