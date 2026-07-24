using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network.Enums;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal static class GamePacketBuilder
{
    public static NetworkMessage BuildItemThrow(
        Location fromLocation,
        ushort itemClientId,
        byte fromStackPosition,
        Location toLocation,
        byte count)
    {
        var message = new NetworkMessage();
        message.AddByte((byte)GameIncomingPacketType.ItemThrow);
        message.AddLocation(fromLocation);
        message.AddUInt16(itemClientId);
        message.AddByte(fromStackPosition);
        message.AddLocation(toLocation);
        message.AddByte(count);

        return message;
    }
}
