using System.Collections.Generic;
using NeoServer.Game.Combat.Services.Attacks.Builders;
using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.World.Algorithms;

namespace NeoServer.Game.Combat.Services.Attacks;

public class AreaAttackService(
    IEventAggregator eventAggregator,
    IMap map,
    CombatBloodPoolService combatBloodPoolService) : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        var damage = DamageBuilder.Build(attackInput);

        PerformAreaAttack(attackInput, damage);

        return Result.Success;
    }

    private void PerformAreaAttack(AttackInput attackInput, CalculatedAttackDamage damage)
    {
        if (!attackInput.Parameters.IsAttackInArea) return;

        var aggressor = attackInput.Aggressor as ICombatActor;

        var areaLocation = aggressor.Location;

        areaLocation =
            areaLocation.AddDirectionStep(attackInput.Parameters.NeedDirection
                ? aggressor.Direction
                : Direction.None);

        var area = AreaEffect.Create(areaLocation, attackInput.Parameters.Area);

        var affectedArea = new List<Location>(area.Length);
        var affectedCreatures = new List<ICreature>();

        foreach (var coordinate in area)
        {
            var location = coordinate.Location;
            var tile = map[location];

            // Check if the tile is walkable and clear of obstacles
            if (tile is not IDynamicTile walkableTile || walkableTile.HasFlag(TileFlags.Unpassable) ||
                walkableTile.ProtectionZone)
            {
                continue;
            }

            // Check if the line of sight is clear between aggressor and target location
            if (!SightClear.IsSightClear(map, attackInput.Aggressor.Location, tile.Location, false))
            {
                continue;
            }

            affectedArea.Add(location);

            var targetCreatures = walkableTile.Creatures?.ToArray();
            if (targetCreatures is null)
            {
                continue;
            }

            affectedCreatures.AddRange(targetCreatures);
        }

        eventAggregator.Publish(new CreatureAttackingEvent(aggressor, attackInput.Target,
            attackInput.Parameters.ShootType,
            attackInput.Parameters.Effect, false, Area: affectedArea.ToArray()));

        foreach (var affectedCreature in affectedCreatures)
        {
            if (affectedCreature is not ICombatActor target) continue;
            if (affectedCreature.Equals(aggressor)) continue;

            var unjustifiedAttack =
                target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
                playerAggressor.GetSkull(targetPlayer) is Skull.None;

            var mainDamage = damage.MainDamage;
            mainDamage.Unjustified = unjustifiedAttack;

            InflictDamage(damage, mainDamage, target, aggressor);

            CreateBloodPool(damage, target);
        }
    }

    private static void InflictDamage(CalculatedAttackDamage damage, CombatDamage mainDamage, ICombatActor target,
        IThing aggressor)
    {
        if (damage.ExtraDamage.Damage > 0)
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            target.TakeDamage(aggressor, damages);
            return;
        }

        target.TakeDamage(aggressor, damage.MainDamage);
    }

    private void CreateBloodPool(CalculatedAttackDamage damage, IThing target)
    {
        if (damage.MainDamage is { Damage: > 0, IsElementalDamage: false })
        {
            combatBloodPoolService.CreateSplash(target as ICombatActor, damage.MainDamage);
            return;
        }

        if (damage.ExtraDamage is { Damage: > 0, IsElementalDamage: false })
        {
            combatBloodPoolService.CreateSplash(target as ICombatActor, damage.ExtraDamage);
        }
    }
}