using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class QuestLinePacket(IReadOnlyNetworkMessage message) : IncomingPacket
{
    public ushort QuestId { get; } = message.GetUInt16();
}