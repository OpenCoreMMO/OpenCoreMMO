using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Common.Contracts.World;

public interface IPathFinder
{
    IMap Map { get; set; }

    (bool Founded, Direction[] Directions) Find(Location.Structs.Location startPosition, Location.Structs.Location targetPosition,
        FindPathParams fpp);

    (bool Founded, Direction[] Directions) Find(ICreature creature, Location.Structs.Location target,
        FindPathParams findPathParams,
        ITileEnterRule tileEnterRule);

    (bool Founded, Direction[] Directions) Find(ICreature creature, Location.Structs.Location target,
        ITileEnterRule tileEnterRule);

    Direction FindRandomStep(ICreature creature, ITileEnterRule rule);

    Direction FindRandomStep(ICreature creature, ITileEnterRule rule, Location.Structs.Location origin,
        int maxStepsFromOrigin = 1);
}