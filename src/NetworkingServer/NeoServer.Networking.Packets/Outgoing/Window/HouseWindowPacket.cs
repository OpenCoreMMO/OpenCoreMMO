using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Window;

public class HouseWindowPacket(uint windowTextId, string text) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.HouseWindow);
        message.AddByte(0x00); // door id unused for guest/subowner lists
        message.AddUInt32(windowTextId);
        message.AddString(text ?? string.Empty);
    }
}
