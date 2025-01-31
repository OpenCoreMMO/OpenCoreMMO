using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureLightPacket(IPlayer player) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CreatureLight;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(player.CreatureId);
        message.AddByte(player.LightBrightness); // light level
        message.AddByte(player.LightColor); // color
    }
}