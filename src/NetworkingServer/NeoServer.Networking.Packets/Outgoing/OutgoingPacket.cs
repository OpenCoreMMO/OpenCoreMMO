using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing;

public abstract class OutgoingPacket : IOutgoingPacket
{
    public abstract byte PacketType { get; }
    public virtual bool Disconnect { get; protected set; } = false;
    public abstract void WriteToMessage(INetworkMessage message);
}