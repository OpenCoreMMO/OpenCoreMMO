using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming.Player;

public class PlayerChangeOutFitPacket : IncomingPacket
{
    public PlayerChangeOutFitPacket(IReadOnlyNetworkMessage message)
    {
        Outfit = new Outfit
        {
            LookType = message.GetUInt16(),
            Head = message.GetByte(),
            Body = message.GetByte(),
            Legs = message.GetByte(),
            Feet = message.GetByte(),
            Addon = message.GetByte()
        };
    }

    public IOutfit Outfit { get; set; }
}