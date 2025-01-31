using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Networking.Packets.Outgoing.Map;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Scripts;
using System.Collections.Generic;
using System.Linq;

namespace NeoServer.Server.Events.Creature;

public class CreatureMovedEventHandler(IGameServer game, IScriptManager scriptManager)
{
    public void Execute(IWalkableCreature creature, ICylinder cylinder)
    {
        if (cylinder.IsNull()) return;
        if (cylinder.TileSpectators.IsNull()) return;
        if (creature.IsNull()) return;

        var toTile = cylinder.ToTile;
        var fromTile = cylinder.FromTile;
        if (toTile.IsNull()) return;
        if (fromTile.IsNull()) return;

        var toDirection = fromTile.Location.DirectionTo(toTile.Location, true);

        MoveCreature(toDirection, creature, cylinder);
    }

    private void MoveCreature(Direction toDirection, IWalkableCreature creature, ICylinder cylinder)
    {
        var fromLocation = cylinder.FromTile.Location;
        var toLocation = cylinder.ToTile.Location;
        var fromTile = cylinder.FromTile;

        if (creature is IMonster && creature.IsInvisible) return;

        foreach (var cylinderSpectator in cylinder.TileSpectators)
        {
            var spectator = cylinderSpectator.Spectator;

            if (spectator is not IPlayer player) continue;

            if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) continue;

            if (TryMoveMyself(creature, cylinder, player, fromLocation, toLocation, connection, fromTile,
                    cylinderSpectator)) continue;

            if (player.CanSee(creature) && player.CanSee(fromLocation) &&
                player.CanSee(toLocation)) //spectator can see old and new location
            {
                MoveCreature(creature, fromLocation, toLocation, connection, fromTile, cylinderSpectator, player);

                connection.Send();

                continue;
            }

            if (player.CanSee(creature) &&
                player.CanSee(fromLocation)) //spectator can see old position but not the new
            {
                //happens when player leaves spectator's view area
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile.Location,
                    cylinderSpectator.FromStackPosition));
                connection.Send();

                continue;
            }

            if (!player.CanSee(creature) || !player.CanSee(toLocation)) continue;

            //happens when player enters spectator's view area
            connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature,
                cylinderSpectator.ToStackPosition));

            connection.OutgoingPackets.Enqueue(
                player.KnowsCreatureWithId(creature.CreatureId) ?
                new AddKnownCreaturePacket(player, creature) :
                new AddUnknownCreaturePacket(player, creature));

            connection.Send();
        }

        scriptManager.MoveEvents.CreatureMove(creature, cylinder.FromTile.Location, cylinder.ToTile.Location);
    }

    private static void MoveCreature(IWalkableCreature creature, Location fromLocation, Location toLocation,
        IConnection connection, ITile fromTile, ICylinderSpectator cylinderSpectator, IPlayer player)
    {
        if (fromLocation.Z != toLocation.Z)
        {
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile.Location,
                cylinderSpectator.FromStackPosition));
            connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature,
                cylinderSpectator.ToStackPosition));

            connection.OutgoingPackets.Enqueue(
                player.KnowsCreatureWithId(creature.CreatureId) ?
                new AddKnownCreaturePacket(player, creature) :
                new AddUnknownCreaturePacket(player, creature));

            return;
        }

        connection.OutgoingPackets.Enqueue(new CreatureMovedPacket(fromLocation, toLocation,
            cylinderSpectator.FromStackPosition));
    }

    private bool TryMoveMyself(ICreature creature, ICylinder cylinder, IPlayer player,
        Location fromLocation, Location toLocation, IConnection connection, ITile fromTile,
        ICylinderSpectator cylinderSpectator)
    {
        if (player.CreatureId != creature.CreatureId) return false;

        if (cylinderSpectator.FromStackPosition >= 10)
        {
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player.Location, game.Map.GetDescriptionFromPlayer(player).ToArray()));
            connection.Send();
            return true;
        }

        if (cylinder.IsTeleport)
        {
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile.Location,
                cylinderSpectator.FromStackPosition));
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player.Location, game.Map.GetDescriptionFromPlayer(player).ToArray()));

            if (fromTile is IDynamicTile fromDynamicTile && fromDynamicTile.HasTeleport(out _))
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(toLocation, EffectT.BubbleBlue));

            connection.Send();
            return true;
        }

        if (fromLocation.Z == 7 && toLocation.Z >= 8)
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile.Location,
                cylinderSpectator.FromStackPosition));
        else
            connection.OutgoingPackets.Enqueue(new CreatureMovedPacket(fromLocation,
                toLocation, cylinderSpectator.FromStackPosition));

        if (toLocation.Z > fromLocation.Z)
            CreatureMovedDown(connection, fromLocation, toLocation, game.Map, creature);
        else if (toLocation.Z < fromLocation.Z)
            CreatureMovedUp(connection, fromLocation, toLocation, game.Map, creature);

        if (toLocation.Y > fromLocation.Y)
            connection.OutgoingPackets.Enqueue(
                    new MapSliceSouthPacket(
                        game.Map.GetDescription(creature, (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX),
                            (ushort)(toLocation.Y + MapViewPort.MaxClientViewPortY + 1), toLocation.Z, windowSizeY: 1)
                        .ToArray()));
        else if (toLocation.Y < fromLocation.Y)
            connection.OutgoingPackets.Enqueue(
                new MapSliceNorthPacket(
                    game.Map.GetDescription(creature, (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX),
                        (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY), toLocation.Z, windowSizeY: 1)
                    .ToArray()));

        if (toLocation.X > fromLocation.X)
            connection.OutgoingPackets.Enqueue(
                new MapSliceEastPacket(
                    game.Map.GetDescription(creature, (ushort)(toLocation.X + MapViewPort.MaxClientViewPortX + 1),
                        (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY), toLocation.Z, windowSizeX: 1)
                    .ToArray()));
        else if (toLocation.X < fromLocation.X)
            connection.OutgoingPackets.Enqueue(
                new MapSliceWestPacket(
                    game.Map.GetDescription(creature, (ushort)(toLocation.X - MapViewPort.MaxClientViewPortX),
                        (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY), toLocation.Z, windowSizeX: 1)
                    .ToArray()));

        connection.Send();

        return true;
    }

    public void CreatureMovedDown(IConnection connection, Location fromLocation, Location toLocation, IMap map, ICreature creature)
    {
        var skip = -1;
        var x = (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX);
        var y = (ushort)(fromLocation.Y - MapViewPort.MaxClientViewPortY);

        var floorsDescription = new List<byte>();

        //going from surface to underground
        if (toLocation.Z == 8)
            for (var i = 0; i < 3; ++i)
                floorsDescription.AddRange(map
                    .GetFloorDescription(creature, x, y, (byte)(toLocation.Z + i), 18, 14, -i - 1, ref skip));

        //going further down
        if (toLocation.Z > fromLocation.Z && toLocation.Z is > 8 and < 14)
        {
            skip = -1;
            floorsDescription.AddRange(map
                .GetFloorDescription(creature, x, y, (byte)(toLocation.Z + 2), 18, 14, -3, ref skip).ToArray());
        }

        if (skip >= 0)
        {
            floorsDescription.Add((byte)skip);
            floorsDescription.Add(0xFF);
        }

        connection.OutgoingPackets.Enqueue(new CreatureMovedDownPacket(floorsDescription.ToArray()));

        //east
        connection.OutgoingPackets.Enqueue(
                new MapSliceEastPacket(map.GetDescription(creature, (ushort)(fromLocation.X + MapViewPort.MaxClientViewPortX + 1),
            (ushort)(y - 1), toLocation.Z, 1).ToArray()));

        //south
        connection.OutgoingPackets.Enqueue(
                new MapSliceEastPacket(map.GetDescription(creature, x,
            (ushort)(fromLocation.Y + MapViewPort.MaxClientViewPortY + 1), toLocation.Z, 18, 1).ToArray()));
    }

    public void CreatureMovedUp(IConnection connection, Location fromLocation, Location toLocation, IMap map, ICreature creature)
    {
        var floorsDescription = new List<byte>();

        var skip = -1;
        var x = (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX);
        var y = (ushort)(fromLocation.Y - MapViewPort.MaxClientViewPortY);

        //going from surface to underground
        if (toLocation.Z == 7)
            for (var i = 5; i >= 0; --i)
                floorsDescription.AddRange(map
                    .GetFloorDescription(
                        creature, x, y, (byte)i, (byte)MapViewPort.MaxClientViewPortX * 2 + 2,
                        (byte)MapViewPort.MaxClientViewPortY * 2 + 2, 8 - i, ref skip).ToArray());

        if (toLocation.Z > 7)
            floorsDescription.AddRange(map
                .GetFloorDescription(
                    creature, (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX),
                    (ushort)(fromLocation.Y - MapViewPort.MaxClientViewPortY),
                    (byte)(fromLocation.Z - 3), (byte)MapViewPort.MaxClientViewPortX * 2 + 2,
                    (byte)MapViewPort.MaxClientViewPortY * 2 + 2, 3, ref skip).ToArray());

        if (skip >= 0)
        {
            floorsDescription.Add((byte)skip);
            floorsDescription.Add(0xFF);
        }

        connection.OutgoingPackets.Enqueue(new CreatureMovedUpPacket(floorsDescription.ToArray()));

        //west
        connection.OutgoingPackets.Enqueue(
                new MapSliceWestPacket(map.GetDescription(creature, x, (ushort)(y + 1), toLocation.Z, 1).ToArray()));

        //north
        connection.OutgoingPackets.Enqueue(
                new MapSliceNorthPacket(map
            .GetDescription(creature, x, y, toLocation.Z, (byte)MapViewPort.MaxClientViewPortX * 2 + 2, 1).ToArray()));
    }
}