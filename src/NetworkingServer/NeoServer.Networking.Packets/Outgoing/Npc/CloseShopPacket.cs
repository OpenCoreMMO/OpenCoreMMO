using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Npc;

public class CloseShopPacket : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.CloseShop);
    }
}