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

        scriptManager.MoveEvents.CreatureMove(creature, cylinder.FromTile.Location, cylinder.ToTile.Location);
    }

    private static void MoveCreature(IWalkableCreature creature, Location fromLocation, Location toLocation,
        IConnection connection, ITile fromTile, ICylinderSpectator cylinderSpectator, IPlayer player)
    {
        if (fromLocation.Z != toLocation.Z)
        {
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                cylinderSpectator.FromStackPosition));
            connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature,
                cylinderSpectator.ToStackPosition));

            connection.OutgoingPackets.Enqueue(new AddCreaturePacket(player, creature));

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
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player, game.Map));
            connection.Send();
            return true;
        }

        if (cylinder.IsTeleport)
        {
            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(fromTile,
                cylinderSpectator.FromStackPosition));
            connection.OutgoingPackets.Enqueue(new MapDescriptionPacket(player, game.Map));

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
            connection.OutgoingPackets.Enqueue(new CreatureMovedDownPacket(fromLocation, toLocation, game.Map,
                creature));
        if (toLocation.Z < fromLocation.Z)
            connection.OutgoingPackets.Enqueue(new CreatureMovedUpPacket(fromLocation, toLocation, game.Map,
                creature));

        if (fromLocation.GetSqmDistanceX(toLocation) != 0 || fromLocation.GetSqmDistanceY(toLocation) != 0)
            connection.OutgoingPackets.Enqueue(new MapPartialDescriptionPacket(creature, fromLocation,
                toLocation, Direction.None, game.Map));

        connection.Send();

        return true;
    }
}