using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.World.Events;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Networking.EventHandlers.World.Tiles;

public class ThingRemovedFromTileEventHandler(IGameServer game, IScriptManager scriptManager)
    : INetworkingEventHandler<ThingRemovedFromTileEvent>
{
    public void Handle(ThingRemovedFromTileEvent @event)
    {
        Execute(@event.Thing, @event.Cylinder);
    }

    private void Execute(IThing thing, ICylinder cylinder)
    {
        if (Guard.AnyNull(cylinder, cylinder.TileSpectators, thing)) return;

        var tile = cylinder.FromTile;
        if (tile.IsNull()) return;

        foreach (var spectator in cylinder.TileSpectators)
        {
            var spectatorCreature = spectator.Spectator;

            if (spectatorCreature is not IPlayer player) continue;
            if (!spectatorCreature.CanSee(thing.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectatorCreature.CreatureId, out var connection)) continue;

            if (player.IsDead && !Equals(thing, player)) continue;

            var stackPosition = spectator.FromStackPosition;

            // if the player is not dead, show a puff effect
            if (thing is IPlayer { IsDead: false } or IMonster { IsSummon: true })
            {
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(tile.Location, EffectT.Puff));
            }

            // if the monster was killed by another monster, show a puff effect
            if (thing is Monster { KilledByAnotherMonster: true })
            {
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(tile.Location, EffectT.Puff));
            }

            if (thing is ICreature creatureThing && spectatorCreature.CanSee(creatureThing))
            {
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(tile, stackPosition));
            }

            connection.Send();
        }

        scriptManager.MoveEvents.ItemMove(thing as IItem, cylinder.ToTile, false);
    }
}