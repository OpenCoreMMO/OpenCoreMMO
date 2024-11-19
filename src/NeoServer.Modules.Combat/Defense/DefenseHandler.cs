using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Modules.Combat.Defense.MonsterDefense;
using NeoServer.Modules.Combat.Defense.PlayerDefense;

namespace NeoServer.Modules.Combat.Defense;

public class DefenseHandler(
    PlayerDefenseHandler playerDefenseHandler,
    MonsterDefenseHandler monsterDefenseHandler,
    IEventBus eventBus) : ISingleton
{
    public void Handle(IThing aggressor, ICombatActor victim, CombatDamageList damageList)
    {
        if (victim is null) return;
        if (victim.IsDead) return;
        if (!victim.CanBeAttacked) return;

        if (victim is IPlayer player)
        {
            if (aggressor?.Equals(player) ?? false) return;
            if (!player.CanBeAttacked) return;
            if (player.IsDead) return;

            playerDefenseHandler.Handle(aggressor, player, damageList);
        }

        if (victim is IMonster monster)
        {
            monsterDefenseHandler.Handle(aggressor, monster, damageList);
        }

        eventBus.Publish(new CreatureDiedEvent(victim, aggressor));
    }
}