using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerSkullUpdatedEventHandler(IMap map, IGameCreatureManager creatureManager) : INetworkEventHandler<ICreature>
{
    public void Execute(IPlayer player)
    {
        foreach (var spectator in map.GetPlayersAtPositionZone(player.Location))
        {
            if(spectator is not IPlayer spectatorPlayer) continue;

            if (!creatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            
            connection.Send(new PlayerSkullPacket
            {
                Player = player,
                Spectator = spectatorPlayer
            });
        }
    }
    
    public void Subscribe(ICreature entity)
    {
        if (entity is not IPlayer player) return;
        player.OnSkullUpdated += Execute;
    }
    public void Unsubscribe(ICreature entity)
    {
        if (entity is not IPlayer player) return;
        player.OnSkullUpdated -= Execute;
    }
}