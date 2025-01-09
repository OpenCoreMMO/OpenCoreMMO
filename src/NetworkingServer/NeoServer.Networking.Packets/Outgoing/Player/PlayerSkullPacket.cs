using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerSkullPacket: OutgoingPacket
{
    public required IPlayer Player { get; init; }
    public IPlayer Spectator { get; init; }
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.CreatureSkull);
        message.AddUInt32(Player.CreatureId);
        
        message.AddByte((byte)Player.GetSkull(Spectator));
    }
}