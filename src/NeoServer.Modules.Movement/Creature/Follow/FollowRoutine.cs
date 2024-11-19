using NeoServer.BuildingBlocks.Application;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Modules.Movement.Creature.Follow;

public class FollowRoutine : ISingleton
{
    private readonly FollowService _followService;

    public FollowRoutine(FollowService followService)
    {
        _followService = followService;
    }

    public void Run(ICreature creature)
    {
        if (creature is not IWalkableCreature walkableCreature) return;
        if (walkableCreature.Following is null) return;
        if (walkableCreature.IsNextTo(walkableCreature.Following)) return;

        _followService.Follow(walkableCreature, walkableCreature.Following);
    }
}