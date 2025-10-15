using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Models.Bases;

namespace NeoServer.Domain.Creatures.Monster;

public abstract class WalkableMonster : CombatActor, IWalkableMonster
{
    protected WalkableMonster(ICreatureType type, IMapTool mapTool, IOutfit outfit = null, uint healthPoints = 0) :
        base(type, mapTool, outfit,
            healthPoints)
    {
    }

    public virtual IMonsterType Metadata => CreatureType as IMonsterType;
    public override ITileEnterRule TileEnterRule => MonsterEnterTileRule.Rule;

    public bool DoRandomStep()
    {
        StopFollowing();
        //StopAttack();

        Cooldowns.Start(CooldownType.Awaken, 10000);

        if (IsDead || HasFollowPath) return false;

        var direction = GetRandomStep();

        if (direction == Direction.None) return false;

        TryWalkTo(direction);

        return true;
    }

    internal void EscapeFromEnemy()
    {
        StopFollowing();
        
        if (CurrentTarget is null) return;

        if (IsDead) return;
        if (MapTool?.PathFinder is null) return;

        var result = MapTool.PathFinder.Find(this, CurrentTarget.Location, FindPathParams.EscapeParams, TileEnterRule);

        if (!result.Found) return;

        TryWalkTo(result.Directions);
    }

    public void MoveAroundEnemy(ICreature enemy)
    {
        if (!Attacking) return;

        if (!Cooldowns.Expired(CooldownType.MoveAroundEnemy)) return;
        Cooldowns.Start(CooldownType.MoveAroundEnemy, (uint)GameRandom.Random.Next(3000, maxValue: 5000));

        var direction = GetRandomStep();
        if (direction == Direction.None) return;

        var nextLocation = Location.GetNextLocation(direction);

        var targetLocation = enemy.Location;

        var tooFar = targetLocation.GetMaxSqmDistance(nextLocation) > Metadata.MaxRangeDistanceAttack;
        
        var hasSightClear = MapTool.SightClearChecker?.Invoke(Location, CurrentTarget.Location, false) ?? false;

        if (Metadata.HasDistanceAttack && !HasFollowPath && hasSightClear && !tooFar)
        {
            TryWalkTo(direction);
            return;
        }

        if (targetLocation.GetMaxSqmDistance(nextLocation) > PathSearchParams.MaxTargetDist) return;
        TryWalkTo(direction);
    }
}