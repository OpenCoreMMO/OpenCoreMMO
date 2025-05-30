using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Npc;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature.Npcs;

public class NpcCloseShopEventHandler(IGameServer game)
{
    public void Execute(ISociableCreature to)
    {
        if (!game.CreatureManager.GetPlayerConnection(to.CreatureId, out var connection)) return;
        connection.OutgoingPackets.Enqueue(new CloseShopPacket());
        connection.Send();
    }
}