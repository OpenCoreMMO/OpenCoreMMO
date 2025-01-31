using System.Collections.Generic;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Common.Contracts.World;

public delegate void PlaceCreatureOnMap(IWalkableCreature creature, ICylinder cylinder);

public delegate void RemoveThingFromTile(IThing thing, ICylinder cylinder);

public delegate void MoveCreatureOnFloor(IWalkableCreature creature, ICylinder cylinder);

public delegate void AddThingToTile(IThing thing, ICylinder cylinder);

public delegate void UpdateThingOnTile(IThing thing, ICylinder cylinder);

public delegate void FailedMoveThing(IThing thing, InvalidOperation error);

public interface IMap
{
    ITile this[Location.Structs.Location location] { get; }
    ITile this[ushort x, ushort y, byte z] { get; }

    event PlaceCreatureOnMap OnCreatureAddedOnMap;
    event RemoveThingFromTile OnThingRemovedFromTile;
    event MoveCreatureOnFloor OnCreatureMoved;
    event FailedMoveThing OnThingMovedFailed;
    event AddThingToTile OnThingAddedToTile;
    event UpdateThingOnTile OnThingUpdatedOnTile;
    IList<byte> GetDescriptionFromPlayer(IPlayer player);

    IList<byte> GetDescription(IThing thing, ushort fromX, ushort fromY, byte currentZ,
        byte windowSizeX = 18, byte windowSizeY = 14);

    bool ArePlayersAround(Location.Structs.Location location);
    void PlaceCreature(ICreature creature);
    ITile GetNextTile(Location.Structs.Location fromLocation, Direction direction);

    IList<byte> GetFloorDescription(IThing thing, ushort fromX, ushort fromY, byte currentZ, byte width,
        byte height, int verticalOffset, ref int skip);

    IEnumerable<ICreature> GetPlayersAtPositionZone(Location.Structs.Location location);

    bool IsInRange(Location.Structs.Location start, Location.Structs.Location current, Location.Structs.Location target,
        FindPathParams fpp);

    HashSet<ICreature> GetCreaturesAtPositionZone(Location.Structs.Location location,
        Location.Structs.Location toLocation);

    void PropagateAttack(ICombatActor actor, CombatDamage damage, AffectedLocation[] area);
    void MoveCreature(IWalkableCreature creature);
    void CreateBloodPool(ILiquid liquid, IDynamicTile tile);
    ITile GetTileDestination(ITile tile);
    bool TryMoveCreature(ICreature creature, Location.Structs.Location toLocation);
    void RemoveCreature(ICreature creature);

    void SwapCreatureBetweenSectors(ICreature creature, Location.Structs.Location fromLocation,
        Location.Structs.Location toLocation);

    HashSet<ICreature> GetSpectators(Location.Structs.Location fromLocation, Location.Structs.Location toLocation,
        bool onlyPlayers = false);

    HashSet<ICreature> GetSpectators(Location.Structs.Location fromLocation, bool onlyPlayers = false);
    IEnumerable<ICreature> GetCreaturesAtPositionZone(Location.Structs.Location location, bool onlyPlayers = false);
    bool CanGoToDirection(ICreature creature, Direction direction, ITileEnterRule rule);
    ITile GetTile(Location.Structs.Location location);
    ITile GetFinalTile(ITile toTile);
    void ReplaceTile(ITile newTile);
}