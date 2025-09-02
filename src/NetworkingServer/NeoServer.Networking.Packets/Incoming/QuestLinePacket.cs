using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class QuestLinePacket: IncomingPacket
{
    public QuestLinePacket(IReadOnlyNetworkMessage message)
    {
        QuestId = message.GetUInt16();
    }

    public ushort QuestId { get;  }
}