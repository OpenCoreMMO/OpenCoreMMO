using NeoServer.BuildingBlocks.Application;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.World.Map;
using NeoServer.Modules.Movement.Creature.Walk;

namespace NeoServer.Modules.Movement.Creature.Follow;

public class FollowService(IPathFinder pathFinder, CreatureWalkScheduler walkScheduler) : ISingleton
{
    public void Follow(IWalkableCreature creature, ICreature targetCreature)
    {
        if (Guard.IsNull(creature)) return;

        if (creature is ICombatActor { IsDead: true })
        {
            creature.StopFollowing();
            return;
        }

        if (creature.HasNoSpeed) return;

        if (creature.IsFollowing)
        {
            StartFollowing(creature, targetCreature);
            return;
        }

        StartFollowing(creature, targetCreature);
        
        //OnStartedFollowing?.Invoke(this, creature, fpp);
    }

    private bool StartFollowing(IWalkableCreature creature, ICreature targetCreature)
    {
        creature.Follow(targetCreature);
        if (!creature.CanSee(targetCreature.Location, 9, 9))
        {
            StopFollowing(creature);
        }

        var result = pathFinder.Find(creature, targetCreature.Location, creature.PathSearchParams,
            creature.TileEnterRule);

        if (!result.Founded)
        {
            return true;
        }
        
        creature.AddSteps(result.Directions);
        
        // we schedule the steps here to allow the creature to start walking after start following has been called.
        walkScheduler.ScheduleSteps(creature);
        return false;
    }

    public void StopFollowing(IWalkableCreature creature)
    {
        creature.StopFollowing();        
    }
}