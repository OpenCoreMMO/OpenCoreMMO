using NeoServer.Domain.Combat.Services.Attacks.Builders;
using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Services;
using NeoServer.Domain.World.Algorithms;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class AreaAttackService(
    IEventAggregator eventAggregator,
    IMap map,
    BloodPoolService bloodPoolService,
    MagicFieldService magicFieldService,
    ConditionAttackService conditionAttackService) : IAttackService
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


        var area = attackInput.Parameters.CoordinateArea ??
                   AreaEffect.Create(areaLocation, attackInput.Parameters.Area);

        var affectedArea = new List<Location>(area.Length);
        var affectedCreatures = new List<ICreature>();

        foreach (var coordinate in area)
        {
            var location = coordinate.Location;
            var tile = map[location];

            // Check if the tile is walkable and clear of obstacles
            if (tile is not IDynamicTile walkableTile || walkableTile.HasFlag(TileFlags.Unpassable) ||
                walkableTile.ProtectionZone)
                continue;

            // Check if the line of sight is clear between aggressor and target location
            if (!SightClear.IsSightClear(map, attackInput.Aggressor.Location, tile.Location, false)) continue;

            affectedArea.Add(location);

            if (attackInput.Parameters.FieldAttack)
            {
                CreateMagicField(attackInput, tile);
            }

            var targetCreatures = walkableTile.Creatures?.ToArray();
            if (targetCreatures is null) continue;

            affectedCreatures.AddRange(targetCreatures);
        }

        eventAggregator.Publish(new CreatureAttackingEvent(aggressor, attackInput.Target,
            attackInput.Parameters.ShootType,
            attackInput.Parameters.Effect, false, affectedArea.ToArray()));

        foreach (var affectedCreature in affectedCreatures)
        {
            if (affectedCreature is not ICombatActor target) continue;
            if (affectedCreature.Equals(aggressor)) continue;

            var unjustifiedAttack =
                target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
                playerAggressor.GetSkull(targetPlayer) is Skull.None;

            var mainDamage = damage.MainDamage;

            if (mainDamage is { Damage: > 0 })
            {
                mainDamage.Unjustified = unjustifiedAttack;

                var wasDamaged = InflictDamage(damage, mainDamage, target, aggressor);

                if (wasDamaged) conditionAttackService.Execute(attackInput);

                CreateBloodPool(damage, target);
            }
            else
            {
                conditionAttackService.Execute(attackInput);
            }
        }
    }

    private void CreateMagicField(AttackInput attackInput, ITile tile)
    {
        var magicFieldType = attackInput.Parameters.DamageType switch
        {
            DamageType.Earth => MagicFieldType.Poison,
            DamageType.Energy => MagicFieldType.Energy,
            DamageType.Fire => MagicFieldType.Fire,
            _ => MagicFieldType.None
        };

        magicFieldService.AddToGround(tile, magicFieldType);
    }

    private static bool InflictDamage(CalculatedAttackDamage damage, CombatDamage mainDamage, ICombatActor target,
        IThing aggressor)
    {
        if (damage.ExtraDamage?.Damage > 0)
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            return target.TakeDamage(aggressor, damages);
        }

        return target.TakeDamage(aggressor, damage.MainDamage);
    }

    private void CreateBloodPool(CalculatedAttackDamage damage, IThing target)
    {
        if (damage.MainDamage is { Damage: > 0, IsElementalDamage: false })
        {
            bloodPoolService.CreateSplash(target as ICombatActor, damage.MainDamage);
            return;
        }

        if (damage.ExtraDamage is { Damage: > 0, IsElementalDamage: false })
            bloodPoolService.CreateSplash(target as ICombatActor, damage.ExtraDamage);
    }
}