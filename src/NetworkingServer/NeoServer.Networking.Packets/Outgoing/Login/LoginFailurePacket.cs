using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class LoginFailurePacket(string text) : OutgoingPacket
{
    public override byte PacketType => (byte)LoginOutgoingPacketType.LoginFailed;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddString(text);
    }
}