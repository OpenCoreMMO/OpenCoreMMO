using NeoServer.BuildingBlocks.Application;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Modules.Combat.Defense.MonsterDefense;

public class MonsterDefenseHandler:ISingleton
{
    public void Handle(IThing aggressor, IMonster monster, CombatDamageList damageList)
    {
        if (aggressor?.Equals(monster) ?? false) return;
        if (!monster.CanBeAttacked) return;
        if (monster.IsDead) return;

        if (aggressor is ICreature creature) monster.SetAsEnemy(creature);

        ProcessDamageList(damageList, monster);

        monster.ProcessDamage(aggressor, monster, damageList);
    }

    private void ProcessDamageList(CombatDamageList damageList, ICombatActor player)
    {
        for (var i = 0; i < damageList.Damages.Length; i++)
        {
            damageList.Damages[i] = MonsterBlockAttack.TryBlock(player, damageList.Damages[i]);
            if (damageList.Damages[i].Damage <= 0) continue;

            if (damageList.Damages[i].Damage > player.HealthPoints)
                damageList.Damages[i].SetNewDamage((ushort)player.HealthPoints);
        }
    }
}