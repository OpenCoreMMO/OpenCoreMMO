using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Events.Tiles;

public class ThingRemovedFromTileEventHandler(IGameServer game, IScriptManager scriptManager)
{
    public void Execute(IThing thing, ICylinder cylinder)
    {
        if (Guard.AnyNull(cylinder, cylinder.TileSpectators, thing)) return;

        var tile = cylinder.FromTile;
        if (tile.IsNull()) return;

        foreach (var spectator in cylinder.TileSpectators)
        {
            var creature = spectator.Spectator;

            if (creature is not IPlayer player) continue;
            if (!creature.CanSee(thing.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) continue;

            if (player.IsDead && !Equals(thing, player)) continue;

            var stackPosition = spectator.FromStackPosition;

            if (thing is IPlayer { IsDead: false } or IMonster { IsSummon: true })
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(tile.Location, EffectT.Puff));

            connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(tile.Location, stackPosition));

            connection.Send();
        }

        scriptManager.MoveEvents.ItemMove(thing as IItem, cylinder.ToTile, false);
    }
}