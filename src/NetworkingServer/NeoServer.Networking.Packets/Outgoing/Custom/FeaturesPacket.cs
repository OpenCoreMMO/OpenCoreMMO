using System;
using System.Linq;
using NeoServer.Server.Common.Contracts.Network;
using static NeoServer.Networking.Packets.Outgoing.Custom.FeaturesPacket;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class FeaturesPacket(params Feature[] features) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ExtendedFeature;

    public override void WriteToMessage(INetworkMessage message)
    {
        var allFeatures = Enum.GetValues<Feature>();

        message.AddByte(PacketType);
        message.AddUInt16((ushort)allFeatures.Length);

        foreach (var feature in features)
        {
            message.AddByte((byte)feature);
            message.AddByte((byte)(features.Contains(feature) ? 1 : 0));
        }
    }

    public enum Feature : byte
    {
        GameExtendedOpcode = 80,
        GameEnvironmentEffect = 13,
        GameExtendedClientPing = 25,
        GameItemTooltip = 93
    }
}