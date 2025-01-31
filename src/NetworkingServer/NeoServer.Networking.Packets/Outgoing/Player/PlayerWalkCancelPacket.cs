using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerWalkCancelPacket(IPlayer player) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.PlayerWalkCancel;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte((byte)player.Direction);
    }
}