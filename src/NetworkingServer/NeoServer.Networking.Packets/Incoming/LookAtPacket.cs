using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class LookAtPacket : IncomingPacket
{
    public LookAtPacket(IReadOnlyNetworkMessage message)
    {
        Location = message.GetLocation();
        message.SkipBytes(2); //sprite id
        StackPosition = message.GetByte();
    }

    public Location Location { get; set; }
    public byte StackPosition { get; set; }
}