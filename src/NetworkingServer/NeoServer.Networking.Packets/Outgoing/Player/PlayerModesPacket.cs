using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerModesPacket(IPlayer player) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.PlayerModes;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte((byte)player.FightMode);
        message.AddByte((byte)player.ChaseMode);
        message.AddByte(player.SecureMode);
    }
}