using System.Collections.Generic;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class FeaturesPacket : OutgoingPacket
{
    public required bool GameExtendedOpcode { get; init; }
    public required bool GameEnvironmentEffect { get; init; }
    public required bool GameExtendedClientPing { get; init; }
    public required bool GameItemTooltip { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(0x43);
        message.AddUInt16(4);

        var features = new Dictionary<byte, bool>
        {
            [(byte)Feature.GameExtendedOpcode] = GameExtendedOpcode,
            [(byte)Feature.GameEnvironmentEffect] = GameEnvironmentEffect,
            [(byte)Feature.GameExtendedClientPing] = GameExtendedClientPing,
            [(byte)Feature.GameItemTooltip] = GameItemTooltip
        };

        foreach (var feature in features)
        {
            message.AddByte(feature.Key);
            message.AddByte((byte)(feature.Value ? 1 : 0));
        }
    }

    private enum Feature : byte
    {
        GameExtendedOpcode = 80,
        GameEnvironmentEffect = 13,
        GameExtendedClientPing = 25,
        GameItemTooltip = 93
    }
}