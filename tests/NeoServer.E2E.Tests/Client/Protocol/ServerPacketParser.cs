using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Networking.Packets.Outgoing;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal readonly record struct TileItemPacket(
    GameOutgoingPacketType Opcode,
    Location Location,
    byte StackPosition);

internal static class ServerPacketParser
{
    public static TileItemPacket ParseTileItemPacket(byte[] decryptedPayload)
    {
        var packets = ParseTileItemPackets(decryptedPayload);
        return packets[0];
    }

    public static IReadOnlyList<TileItemPacket> ParseTileItemPackets(byte[] decryptedPayload)
    {
        var packets = new List<TileItemPacket>();
        var message = new ReadOnlyNetworkMessage(decryptedPayload, decryptedPayload.Length);
        message.SkipBytes(2);

        while (message.BytesRead < decryptedPayload.Length)
        {
            if (!TryParseNextTileItemPacket(message, out var packet))
            {
                break;
            }

            packets.Add(packet);
        }

        return packets;
    }

    private static bool TryParseNextTileItemPacket(ReadOnlyNetworkMessage message, out TileItemPacket packet)
    {
        packet = default;

        if (message.BytesRead >= message.Length)
        {
            return false;
        }

        var opcode = (GameOutgoingPacketType)message.GetByte();

        if (opcode is not (
            GameOutgoingPacketType.RemoveAtStackPos or
            GameOutgoingPacketType.TransformThing or
            GameOutgoingPacketType.AddAtStackPos))
        {
            return false;
        }

        var location = message.GetLocation();
        var stackPosition = message.GetByte();

        if (opcode is GameOutgoingPacketType.TransformThing or GameOutgoingPacketType.AddAtStackPos)
        {
            message.SkipBytes(3);
        }

        packet = new TileItemPacket(opcode, location, stackPosition);
        return true;
    }
}
