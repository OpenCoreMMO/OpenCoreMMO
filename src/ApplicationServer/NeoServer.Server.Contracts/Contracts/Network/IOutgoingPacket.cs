namespace NeoServer.Server.Common.Contracts.Network;

public interface IOutgoingPacket
{
    byte PacketType { get; }
    void WriteToMessage(INetworkMessage message);
}