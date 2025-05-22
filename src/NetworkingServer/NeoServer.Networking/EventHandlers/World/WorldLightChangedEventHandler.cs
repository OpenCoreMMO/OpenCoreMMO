using NeoServer.Game.Common;
using NeoServer.Game.World;
using NeoServer.Networking.Packets.Outgoing.Map;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.World;

public class WorldLightChangedEventHandler(IGameCreatureManager creatureManager) : INetworkingEventHandler<WorldLightChangedEvent>
{
    public void Handle(WorldLightChangedEvent @event)
    {
        var players = creatureManager.GetAllLoggedPlayers();
        
        foreach (var player in players)
        {
            if (!creatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;
            
            connection.Send(new WorldLightPacket(@event.Level));
        }
    }
}