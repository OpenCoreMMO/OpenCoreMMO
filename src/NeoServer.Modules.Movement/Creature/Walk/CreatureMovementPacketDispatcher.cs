using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Network;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Networking.Packets.Outgoing.Map;

namespace NeoServer.Modules.Movement.Creature.Walk;

public class CreatureMovementPacketDispatcher(IGameCreatureManager creatureManager, IMap map) : ISingleton
{
    /// <summary>
    /// Sends all movement packets for the given <paramref name="creature"/> and <paramref name="cylinder"/>.
    /// </summary>
    /// <param name="creature">The creature that is moving.</param>
    /// <param name="cylinder">The cylinder of the creature's movement.</param>
    public void Send(IWalkableCreature creature, ICylinder cylinder)
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

            if (!creatureManager.GetPlayerConnection(player.CreatureId, out var connection)) continue;

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
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                    cylinderSpectator.FromStackPosition));
                connection.Send();

                continue;
            }

            if (!player.CanSee(creature) || !player.CanSee(toLocation)) continue;

            //happens when player enters spectator's view area
            connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature,
                cylinderSpectator.ToStackPosition));

            connection.OutgoingPackets.Enqueue(new AddCreaturePacket(player, creature));

            connection.Send();
        }
    }

    private static void MoveCreature(IWalkableCreature creature, Location fromLocation, Location toLocation,
        IConnection connection, ITile fromTile, ICylinderSpectator cylinderSpectator, IPlayer player)
    {
        // Check if there is a change in the Z-axis (vertical movement)
        if (fromLocation.Z != toLocation.Z)
        {
            // Remove the creature from the original location
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                cylinderSpectator.FromStackPosition));
            // Add the creature to the new location
            connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature,
                cylinderSpectator.ToStackPosition));
            // Notify the player of the creature's addition
            connection.OutgoingPackets.Enqueue(new AddCreaturePacket(player, creature));

            return; // Exit after handling vertical movement
        }

        // Handle horizontal movement by sending a packet with the creature's new position
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
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player, map));
            connection.Send();
            return true;
        }

        if (cylinder.IsTeleport)
        {
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                cylinderSpectator.FromStackPosition));
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player, map));

            if (fromTile is IDynamicTile fromDynamicTile && fromDynamicTile.HasTeleport(out _))
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(toLocation, EffectT.BubbleBlue));

            connection.Send();
            return true;
        }

        if (fromLocation.Z == 7 && toLocation.Z >= 8)
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                cylinderSpectator.FromStackPosition));
        else
            connection.OutgoingPackets.Enqueue(new CreatureMovedPacket(fromLocation,
                toLocation, cylinderSpectator.FromStackPosition));

        if (toLocation.Z > fromLocation.Z)
            connection.OutgoingPackets.Enqueue(new CreatureMovedDownPacket(fromLocation, toLocation, map,
                creature));
        if (toLocation.Z < fromLocation.Z)
            connection.OutgoingPackets.Enqueue(new CreatureMovedUpPacket(fromLocation, toLocation, map,
                creature));

        if (fromLocation.GetSqmDistanceX(toLocation) != 0 || fromLocation.GetSqmDistanceY(toLocation) != 0)
            connection.OutgoingPackets.Enqueue(new MapPartialDescriptionPacket(creature, fromLocation,
                toLocation, Direction.None, map));

        connection.Send();

        return true;
    }
}