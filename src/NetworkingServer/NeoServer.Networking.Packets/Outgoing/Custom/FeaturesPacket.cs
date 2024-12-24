using System.Collections.Generic;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class FeaturesPacket : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(0x43);
        message.AddUInt16(4);

        var features = new Dictionary<byte, bool>
        {
            [80] = true,
            [13] = false,
            [25] = true,
            [93] = true
        };
        foreach (var feature in features)
        {
            message.AddByte(feature.Key);
            message.AddByte((byte)(feature.Value ? 1 : 0));
        }
    }
}