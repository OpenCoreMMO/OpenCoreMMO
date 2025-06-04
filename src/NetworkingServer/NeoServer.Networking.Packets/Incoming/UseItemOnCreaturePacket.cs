using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class UseItemOnCreaturePacket : IncomingPacket
{
    public UseItemOnCreaturePacket(IReadOnlyNetworkMessage message)
    {
        FromLocation = message.GetLocation();
        ClientId = message.GetUInt16();
        FromStackPosition = message.GetByte();
        CreatureId = message.GetUInt32();
    }

    public Location FromLocation { get; }
    public ushort ClientId { get; }
    public byte FromStackPosition { get; set; }
    public uint CreatureId { get; }
}