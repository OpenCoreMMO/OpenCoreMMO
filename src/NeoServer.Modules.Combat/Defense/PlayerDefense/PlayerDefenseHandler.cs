using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Modules.Combat.Defense.PlayerDefense;

public class PlayerDefenseHandler():ISingleton
{
    public void Handle(IThing aggressor, IPlayer player, CombatDamageList damageList)
    {
        if (aggressor?.Equals(player) ?? false) return;
        if (!player.CanBeAttacked) return;
        if (player.IsDead) return;

        if (aggressor is ICreature creature) player.SetAsEnemy(creature);

        ProcessDamageList(damageList, player);

        player.ProcessDamage(aggressor, player, damageList);
    }

    private static void ProcessDamageList(CombatDamageList damageList, ICombatActor player)
    {
        for (var i = 0; i < damageList.Damages.Length; i++)
        {
            damageList.Damages[i] = PlayerBlockAttack.TryBlock(player, damageList.Damages[i]);
            if (damageList.Damages[i].Damage <= 0) continue;

            if (damageList.Damages[i].Damage > player.HealthPoints)
                damageList.Damages[i].SetNewDamage((ushort)player.HealthPoints);
        }
    }
}